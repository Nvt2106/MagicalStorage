using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Proxy
{
    /// <summary>
    /// This class provides function to create full proxy type for a given entity type.
    /// A proxy type will:
    /// - Implement IMSEntity interface
    /// - For any normal store property, add a [NotStore] property to store previous value (to track change)
    /// - For any virtual single relation property, add XYZId (Guid) property (for example: virtual property Person -> add PersonId)
    /// - Override getter method for all virtual relation property, which do lazy loading data.
    /// </summary>
    public class MSProxyTypeGenerator
    {
        // Input module builder
        public ModuleBuilder ModuleBuilder { get; private set; }

        // Type of entity which needs to create proxy
        public Type EntityType { get; private set; }

        // List of parent types (which entity type is child)
        public List<Type> ParentEntityTypes { get; private set; }

        // Store proxy type result which is generated from given entity type
        private Type proxyType;

        // Store builders after building a public get/set EntityId (Guid) property
        private MSPropertyBuilderInfo publicPropertyEntityIdBuilders;

        // Store builders after building a public get/set EntityContext (MSEntityContext) property
        private MSPropertyBuilderInfo publicPropertyEntityContextBuilders;

        // Store type builder
        private TypeBuilder proxyTypeBuilder;

        // Store property generator
        private MSProxyPropertyGenerator proxyPropertyGenerator;

        // Store [NotStore] custom builder
        private CustomAttributeBuilder notStoreCustomAttributeBuilder;

        /// <summary>
        /// Construct instance with module builder and entity type.
        /// </summary>
        /// <param name="moduleBuilder">ModuleBuilder</param>
        /// <param name="entityType">Type of entity</param>
        /// <pparam name="parentEntityTypes">List of types which entity type is child</pparam>
        public MSProxyTypeGenerator(ModuleBuilder moduleBuilder, Type entityType, List<Type> parentEntityTypes = null)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(moduleBuilder, "moduleBuilder");
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            if (!(new MSProxyTypeFactory()).CanCreateProxyTypeForType(entityType))
            {
                throw new MSProxyException("Param entityType must be public / class / not sealed / not abstract");
            }

            this.ModuleBuilder = moduleBuilder;
            this.EntityType = entityType;
            this.ParentEntityTypes = parentEntityTypes;

            var ctorInfo = typeof(NotStoreAttribute).GetConstructor(new Type[] { });
            notStoreCustomAttributeBuilder = new CustomAttributeBuilder(ctorInfo, new object[] { });

            GetProxyTypeBuilder();
        }

        /// <summary>
        /// Return proxy type builder.
        /// </summary>
        /// <returns>TypeBuilder</returns>
        public TypeBuilder GetProxyTypeBuilder()
        {
            if (proxyTypeBuilder == null)
            {
                // Init type builder
                proxyTypeBuilder = BuildSkeletonProxyTypeOnly();

                // Init proxy property builder
                proxyPropertyGenerator = new MSProxyPropertyGenerator(proxyTypeBuilder);

                // Build [Required] EntityId property
                publicPropertyEntityIdBuilders = proxyPropertyGenerator.BuildDefaultPublicGetSetProperty(typeof(Guid), MSConstants.NameOfPrimaryIdProperty);
                var ctorInfo = typeof(RequiredAttribute).GetConstructor(new Type[] { });
                var requiredCustomAttributeBuilder = new CustomAttributeBuilder(ctorInfo, new object[] { });
                publicPropertyEntityIdBuilders.Item2.SetCustomAttribute(requiredCustomAttributeBuilder);

                BuildPreviousPropertyForPropertyInfo(publicPropertyEntityIdBuilders.Item2);

                // Build EntityContext property
                publicPropertyEntityContextBuilders = proxyPropertyGenerator.BuildDefaultPublicGetSetProperty(typeof(MSEntityContext), MSConstants.NameOfEntityContextProperty);

                // Build all proxy properties
                foreach (var propertyInfo in this.EntityType.GetProperties())
                {
                    if (IsPropertyForCreatingProxyProperty(propertyInfo))
                    {
                        var flagPropertyBuilder = BuildFlagPropertyToIndicatePersistStorageLoadedForRelationProperty(propertyInfo.Name);

                        BuildProxyPropertyForRelationPropertyInfo(propertyInfo, flagPropertyBuilder);
                    }
                }

                // Build property to store previous values (for tracking change)
                // Exclude EntityType and CollectionOfEntityType
                // Those will be processed in relation entities
                foreach (var propertyInfo in this.EntityType.GetProperties())
                {
                    if (MSPropertyHelper.IsStoreProperty(propertyInfo))
                    {
                        if (MSTypeHelper.IsSingularType(propertyInfo.PropertyType))
                        {
                            BuildPreviousPropertyForPropertyInfo(propertyInfo);
                        }
                    }
                }

                // Build reversed properties to parent
                // It includes reversed property, Guid Id property and Bool flag property
                if (this.ParentEntityTypes != null
                    && this.ParentEntityTypes.Count > 0)
                {
                    foreach (var parentEntityType in this.ParentEntityTypes)
                    {
                        var flagPropertyBuilder = BuildFlagPropertyToIndicatePersistStorageLoadedForRelationProperty(parentEntityType.Name);

                        var parentPropertyIdName = MSNameHelper.NameOfSingularPropertyIdForSingularRelationProperty(parentEntityType.Name);
                        var parentPropertyIdBuilders = proxyPropertyGenerator.BuildDefaultPublicGetSetProperty(typeof(Guid), parentPropertyIdName);
                        BuildPreviousPropertyForPropertyInfo(parentPropertyIdBuilders.Item2);

                        // Build backing private field (variable) for proxy property
                        var backingPrivateFieldBuilder = proxyPropertyGenerator.BuildPrivateField(parentEntityType, MSNameHelper.NameOfBackingPrivateFieldForProperty(parentEntityType.Name));

                        // Override property in super class
                        var propertyBuilder = proxyPropertyGenerator.BuildPublicProperty(parentEntityType, parentEntityType.Name);

                        // Build load data method for lazy loading
                        var loadDataMethodBuilder = BuildProxyLoadDataMethodForSingularProperty(propertyBuilder, backingPrivateFieldBuilder, parentPropertyIdBuilders, flagPropertyBuilder);

                        // Build public get / set methods for proxy property
                        proxyPropertyGenerator.BuildProxyGetMethodForProperty(propertyBuilder, backingPrivateFieldBuilder, loadDataMethodBuilder);
                        proxyPropertyGenerator.BuildProxySetMethodForProperty(propertyBuilder, backingPrivateFieldBuilder, flagPropertyBuilder.Item4);
                    }
                }
            }

            return proxyTypeBuilder;
        }

        /// <summary>
        /// Generate a new proxy type for a given entity type.
        /// </summary>
        /// <returns>Proxy type</returns>
        public Type GetProxyType()
        {
            if (proxyType == null)
            {
                proxyType = proxyTypeBuilder.CreateType();
            }

            return proxyType;
        }

        private bool IsPropertyForCreatingProxyProperty(PropertyInfo propertyInfo)
        {
            bool result = MSPropertyHelper.IsStoreProperty(propertyInfo);
            if (result)
            {
                result = MSPropertyHelper.IsVirtualProperty(propertyInfo);
                if (result)
                {
                    result = (MSTypeHelper.IsEntityType(propertyInfo.PropertyType) || MSTypeHelper.IsCollectionOfEntityType(propertyInfo.PropertyType));
                }
            }
            return result;
        }

        private TypeBuilder BuildSkeletonProxyTypeOnly()
        {
            var proxyTypeName = MSConstants.PrefixNameForProxyType + this.EntityType.Name;
            var typeBuilder = this.ModuleBuilder.DefineType(proxyTypeName, TypeAttributes.Public, this.EntityType, new Type[] { typeof(IMSEntity) });
            return typeBuilder;
        }

        private void BuildProxyPropertyForRelationPropertyInfo(PropertyInfo propertyInfo,
            MSPropertyBuilderInfo flagPropertyBuilder)
        {
            // Build backing private field (variable) for proxy property
            var backingPrivateFieldBuilder = proxyPropertyGenerator.BuildBackingPrivateFieldForPropertyInfo(propertyInfo);

            // Override property in super class
            var propertyBuilder = proxyPropertyGenerator.BuildProxyPropertyForPropertyInfo(propertyInfo);

            // Build load data method for lazy loading
            MethodBuilder loadDataMethodBuilder;
            if (MSPropertyHelper.IsCollectionProperty(propertyBuilder))
            {
                loadDataMethodBuilder = BuildProxyLoadDataMethodForCollectionProperty(propertyBuilder, backingPrivateFieldBuilder, flagPropertyBuilder);
            }
            else
            {
                // Build backing property Id (Guid) for a singular property
                var singularPropertyIdName = MSNameHelper.NameOfSingularPropertyIdForSingularRelationProperty(propertyInfo.Name);
                var singularPropertyIdBuilders = proxyPropertyGenerator.BuildDefaultPublicGetSetProperty(typeof(Guid), singularPropertyIdName);

                // Add custom attributes from parent to proxy Id
                var attrs = propertyInfo.CustomAttributes;
                foreach (var attr in attrs)
                {
                    var args = attr.ConstructorArguments;
                    var objs = new object[args.Count];
                    for (int i = 0; i < objs.Length; i++)
                    {
                        objs[i] = args[i].Value;
                    }
                    var customAttributeBuilder = new CustomAttributeBuilder(attr.Constructor, objs);
                    singularPropertyIdBuilders.Item2.SetCustomAttribute(customAttributeBuilder);
                }

                BuildPreviousPropertyForPropertyInfo(singularPropertyIdBuilders.Item2);

                loadDataMethodBuilder = BuildProxyLoadDataMethodForSingularProperty(propertyBuilder, backingPrivateFieldBuilder, singularPropertyIdBuilders, flagPropertyBuilder);
            }
            
            // Build public get / set methods for proxy property
            proxyPropertyGenerator.BuildProxyGetMethodForProperty(propertyBuilder, backingPrivateFieldBuilder, loadDataMethodBuilder);
            proxyPropertyGenerator.BuildProxySetMethodForProperty(propertyBuilder, backingPrivateFieldBuilder, flagPropertyBuilder.Item4);
        }

        private MSPropertyBuilderInfo BuildFlagPropertyToIndicatePersistStorageLoadedForRelationProperty(string relationPropertyName)
        {
            var flagPropertyName = MSNameHelper.NameOfIsLoadedPropertyForRelationProperty(relationPropertyName);
            var flagProperty = proxyPropertyGenerator.BuildDefaultPublicGetSetProperty(typeof(bool), flagPropertyName);
            flagProperty.Item2.SetCustomAttribute(notStoreCustomAttributeBuilder);
            return flagProperty;
        }

        private void BuildPreviousPropertyForPropertyInfo(PropertyInfo propertyInfo)
        {
            var previousPropertyName = MSNameHelper.PreviousPropertyNameForStoreProperty(propertyInfo.Name);
            var previousProperty = proxyPropertyGenerator.BuildDefaultPublicGetSetProperty(propertyInfo.PropertyType, previousPropertyName);
            previousProperty.Item2.SetCustomAttribute(notStoreCustomAttributeBuilder);
        }

        private MethodBuilder BuildProxyLoadDataMethodForSingularProperty(PropertyBuilder propertyBuilder,
            FieldBuilder backingPrivateFieldBuilder,
            MSPropertyBuilderInfo singularPropertyIdBuilders,
            MSPropertyBuilderInfo flagPropertyBuilder)
        {
            // Method attributes
            var methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;
            var methodBuilder = proxyTypeBuilder.DefineMethod(
                MSNameHelper.NameOfLoadDataMethodForRelationProperty(propertyBuilder.Name), 
                methodAttributes,
                typeof(void),
                new Type[] { });
            // Preparing Reflection instances
            var methodEntityContextGet = typeof(MSEntityContext).GetMethod(
                MSConstants.NameOfGetEntityMethodInEntityContext,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(Type), typeof(Guid) },
                null
                );
            var methodGetTypeFromHandle = typeof(Type).GetMethod(
                "GetTypeFromHandle",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    typeof(RuntimeTypeHandle)
                },
                null
                );
            // Adding parameters
            var gen = methodBuilder.GetILGenerator();
            // Preparing locals
            LocalBuilder flag = gen.DeclareLocal(typeof(Boolean));            
            // Preparing labels
            Label label44 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0); // this
            gen.Emit(OpCodes.Call, flagPropertyBuilder.Item3); // get IsLoaded_
            gen.Emit(OpCodes.Stloc_0); // IsLoaded_
            gen.Emit(OpCodes.Ldloc_0); // flag
            gen.Emit(OpCodes.Brtrue_S, label44); // if flag = true, then go to label 44
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0); // this
            gen.Emit(OpCodes.Ldc_I4_1); // true
            gen.Emit(OpCodes.Call, flagPropertyBuilder.Item4); // Set IsLoaded = true
            gen.Emit(OpCodes.Nop);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, publicPropertyEntityContextBuilders.Item3); // Get method for EntityContext
            gen.Emit(OpCodes.Ldtoken, propertyBuilder.PropertyType);
            gen.Emit(OpCodes.Call, methodGetTypeFromHandle);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, singularPropertyIdBuilders.Item3); // Get method for backing Id property of this property
            gen.Emit(OpCodes.Callvirt, methodEntityContextGet);
            gen.Emit(OpCodes.Castclass, propertyBuilder.PropertyType);
            gen.Emit(OpCodes.Stfld, backingPrivateFieldBuilder);

            gen.Emit(OpCodes.Nop);

            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(label44);
            gen.Emit(OpCodes.Ret);
            // finished
            return methodBuilder;
        }

        private MethodBuilder BuildProxyLoadDataMethodForCollectionProperty(PropertyBuilder propertyBuilder,
            FieldBuilder backingPrivateFieldBuilder,
            MSPropertyBuilderInfo flagPropertyBuilder)
        {
            // Method attributes
            var methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;
            var methodBuilder = proxyTypeBuilder.DefineMethod(
                MSNameHelper.NameOfLoadDataMethodForRelationProperty(propertyBuilder.Name),
                methodAttributes,
                typeof(void),
                new Type[] { });
            // Generic type
            var genericEntityType = propertyBuilder.PropertyType.GenericTypeArguments[0];
            // Preparing Reflection instances
            MethodInfo method1 = flagPropertyBuilder.Item3; // get IsLoaded_
            MethodInfo method2 = flagPropertyBuilder.Item4; // set IsLoaded_
            MethodInfo method3 = publicPropertyEntityIdBuilders.Item3; // get EntityId
            ConstructorInfo ctor4 = typeof(MSCondition).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    typeof(String),
                    typeof(Object),
                    typeof(MSCompareOperator)
                },
                null
                );
            ConstructorInfo ctor5 = typeof(MSConditions).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    typeof(Boolean)
                },
                null
                );
            MethodInfo method6 = typeof(MSConditions).GetMethod(
                "Add",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    typeof(IMSCondition)
                },
                null
                );
            ConstructorInfo ctor7 = typeof(MSPageSetting).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                },
                null
                );
            MethodInfo method8 = publicPropertyEntityContextBuilders.Item3; // get EntityContext
            MethodInfo method9 = typeof(Type).GetMethod(
                "GetTypeFromHandle",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    typeof(RuntimeTypeHandle)
                },
                null
                );
            MethodInfo method10 = typeof(MSEntityContext).GetMethod(
                MSConstants.NameOfSearchEntityMethodInEntityContext,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    typeof(Type),
                    typeof(MSConditions),
                    typeof(MSPageSetting)
                },
                null
                );
            ConstructorInfo ctor11 = typeof(System.Collections.Generic.List<>).MakeGenericType(genericEntityType).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                },
                null
                );
            FieldInfo field12 = backingPrivateFieldBuilder;
            MethodInfo method13 = typeof(System.Collections.Generic.List<>).MakeGenericType(typeof(Object)).GetMethod(
                "get_Count",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                },
                null
                );
            MethodInfo method14 = typeof(System.Collections.Generic.List<>).MakeGenericType(typeof(Object)).GetMethod(
                "get_Item",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    typeof(Int32)
                },
                null
                );
            MethodInfo method15 = typeof(System.Collections.Generic.List<>).MakeGenericType(genericEntityType).GetMethod(
                "Add",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                    genericEntityType
                },
                null
                );
            // Adding parameters
            ILGenerator gen = methodBuilder.GetILGenerator();
            // Preparing locals
            LocalBuilder condition = gen.DeclareLocal(typeof(MSCondition));
            LocalBuilder conditions = gen.DeclareLocal(typeof(MSConditions));
            LocalBuilder setting = gen.DeclareLocal(typeof(MSPageSetting));
            LocalBuilder list = gen.DeclareLocal(typeof(System.Collections.Generic.List<>).MakeGenericType(typeof(Object)));
            LocalBuilder num = gen.DeclareLocal(typeof(Int32));
            LocalBuilder num2 = gen.DeclareLocal(typeof(Int32));
            LocalBuilder flag = gen.DeclareLocal(typeof(Boolean));
            // Preparing labels
            Label label186 = gen.DefineLabel();
            Label label176 = gen.DefineLabel();
            Label label161 = gen.DefineLabel();
            Label label128 = gen.DefineLabel();
            Label label185 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_S, 6);
            gen.Emit(OpCodes.Ldloc_S, 6);
            gen.Emit(OpCodes.Brtrue, label186);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, method2); // Set IsLoaded_ = true
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldstr, MSNameHelper.NameOfSingularPropertyIdForSingularRelationProperty(this.EntityType.Name));
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method3);
            gen.Emit(OpCodes.Box, typeof(Guid));
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Newobj, ctor4);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Newobj, ctor5);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Callvirt, method6);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Newobj, ctor7);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method8);
            gen.Emit(OpCodes.Ldtoken, genericEntityType);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Callvirt, method10);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Stloc_S, 6);
            gen.Emit(OpCodes.Ldloc_S, 6);
            gen.Emit(OpCodes.Brtrue_S, label176);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Newobj, ctor11);
            gen.Emit(OpCodes.Stfld, field12);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Callvirt, method13);
            gen.Emit(OpCodes.Stloc_S, 4);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Stloc_S, 5);
            gen.Emit(OpCodes.Br_S, label161);
            gen.MarkLabel(label128);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, field12);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ldloc_S, 5);
            gen.Emit(OpCodes.Callvirt, method14);
            gen.Emit(OpCodes.Castclass, genericEntityType);
            gen.Emit(OpCodes.Callvirt, method15);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldloc_S, 5);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Add);
            gen.Emit(OpCodes.Stloc_S, 5);
            gen.MarkLabel(label161);
            gen.Emit(OpCodes.Ldloc_S, 5);
            gen.Emit(OpCodes.Ldloc_S, 4);
            gen.Emit(OpCodes.Clt);
            gen.Emit(OpCodes.Stloc_S, 6);
            gen.Emit(OpCodes.Ldloc_S, 6);
            gen.Emit(OpCodes.Brtrue_S, label128);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Br_S, label185);
            gen.MarkLabel(label176);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Stfld, field12);
            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(label185);
            gen.Emit(OpCodes.Nop);
            gen.MarkLabel(label186);
            gen.Emit(OpCodes.Ret);
            // finished
            return methodBuilder;
        }
    }
}
