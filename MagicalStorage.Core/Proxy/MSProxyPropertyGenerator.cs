using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Proxy
{
    /// <summary>
    /// This class provides functions to build private field, build or override public property of super type.
    /// </summary>
    public class MSProxyPropertyGenerator
    {
        // Build proxy property for this proxy type.
        public TypeBuilder TypeBuilder { get; private set; }

        /// <summary>
        /// Constructor to create class instance with given type builder.
        /// </summary>
        /// <param name="typeBuilder">TypeBuilder</param>
        public MSProxyPropertyGenerator(TypeBuilder typeBuilder)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(typeBuilder, "typeBuilder");
            
            this.TypeBuilder = typeBuilder;
        }

        /// <summary>
        /// Build a private field and add to the type.
        /// </summary>
        /// <param name="fieldType">Field type</param>
        /// <param name="fieldName">Field name</param>
        /// <returns>FieldBuilder</returns>
        /// <remarks>
        /// Param typeBuilder, fieldType must not be null; otherwise, exception is thrown.
        /// Param fieldName must be valid identifier; otherwise, exception is thrown.
        /// </remarks>
        public FieldBuilder BuildPrivateField(Type fieldType, string fieldName)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(fieldType, "fieldType");
            if (!MSStringHelper.IsValidIdentifier(fieldName))
            {
                throw new MSProxyException("Param fieldName must not be valid identifier");
            }

            var fieldBuilder = this.TypeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Private);
            return fieldBuilder;
        }

        /// <summary>
        /// Build a private field for a property of entity type and add to the proxy type.
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns>FieldBuilder</returns>
        /// <remarks>
        /// Param typeBuilder, propertyInfo must not be null; otherwise, exception is thrown.
        /// </remarks>
        public FieldBuilder BuildBackingPrivateFieldForPropertyInfo(PropertyInfo propertyInfo)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyInfo, "propertyInfo");
            
            return BuildPrivateField(propertyInfo.PropertyType, MSNameHelper.NameOfBackingPrivateFieldForProperty(propertyInfo.Name));
        }

        /// <summary>
        /// Build a public property and add to the given type.
        /// </summary>
        /// <param name="propertyType">Property type</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>PropertyBuilder</returns>
        /// <remarks>
        /// Param typeBuilder, propertyType must be not null; otherwise, exception is thrown.
        /// Param propertyName must be valid identifier; otherwise, exception is thrown.
        /// </remarks>
        public PropertyBuilder BuildPublicProperty(Type propertyType, string propertyName)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyType, "propertyType");
            if (!MSStringHelper.IsValidIdentifier(propertyName))
            {
                throw new MSProxyException("Param propertyName must not be valid identifier");
            }

            var propertyBuilder = this.TypeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            return propertyBuilder;
        }

        /// <summary>
        /// Build a setter method for a property and set that setter method for the property.
        /// </summary>
        /// <param name="propertyBuilder">PropertyBuilder</param>
        /// <param name="backingPrivateFieldBuilder">Private backing field for that property</param>
        /// <returns>MethodBuilder</returns>
        /// <remarks>
        /// All params must be not null; otherwise, exception is thrown. 
        /// </remarks>
        public MethodBuilder BuildDefaultSetMethodForProperty(PropertyBuilder propertyBuilder, FieldBuilder backingPrivateFieldBuilder)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyBuilder, "propertyBuilder");
            MSParameterHelper.MakeSureInputParameterNotNull(backingPrivateFieldBuilder, "backingPrivateFieldBuilder");
            
            // Method attributes
            var methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.Final // them
                | MethodAttributes.HideBySig;
            var methodBuilder = this.TypeBuilder.DefineMethod(
                MSNameHelper.NameOfSetMethodForProperty(propertyBuilder.Name), 
                methodAttributes, 
                typeof(void), 
                new Type[] { propertyBuilder.PropertyType });
            // Parameter value
            var value = methodBuilder.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = methodBuilder.GetILGenerator();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, backingPrivateFieldBuilder);
            gen.Emit(OpCodes.Ret);
            // Set this method to setter of property
            propertyBuilder.SetSetMethod(methodBuilder);
            // finished
            return methodBuilder;
        }

        /// <summary>
        /// Build getter method and set that getter method for property.
        /// </summary>
        /// <param name="propertyBuilder">PropertyBuilder</param>
        /// <param name="backingPrivateFieldBuilder">Backing private field for the property</param>
        /// <returns>MethodBuilder</returns>
        /// <remarks>All params must be not null; otherwise, exception is thrown</remarks>
        public MethodBuilder BuildDefaultGetMethodForProperty(PropertyBuilder propertyBuilder, FieldBuilder backingPrivateFieldBuilder)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyBuilder, "propertyBuilder");
            MSParameterHelper.MakeSureInputParameterNotNull(backingPrivateFieldBuilder, "backingPrivateFieldBuilder");
            
            // Method attributes
            var methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual // them
                | MethodAttributes.Final // them
                | MethodAttributes.HideBySig;
            var methodBuilder = this.TypeBuilder.DefineMethod(
                MSNameHelper.NameOfGetMethodForProperty(propertyBuilder.Name), 
                methodAttributes, 
                propertyBuilder.PropertyType, 
                new Type[] { });
            // Preparing Reflection instances
            var ctor = typeof(CompilerGeneratedAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{ },
                null
                );
            // Adding custom attributes to method
            // [CompilerGeneratedAttribute]
            methodBuilder.SetCustomAttribute(
                new CustomAttributeBuilder(
                    ctor,
                    new Type[] { }
                    )
                );
            // Adding parameters
            ILGenerator gen = methodBuilder.GetILGenerator();
            // Preparing locals
            LocalBuilder context = gen.DeclareLocal(propertyBuilder.PropertyType);
            // Preparing labels
            Label label9 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, backingPrivateFieldBuilder);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label9);
            gen.MarkLabel(label9);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // Set this method to setter of property
            propertyBuilder.SetGetMethod(methodBuilder);
            // finished
            return methodBuilder;
        }

        /// <summary>
        /// Convenience function to build full public get/set property for a given type.
        /// </summary>
        /// <param name="propertyType">Property type</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>MSPropertyBuilderInfo</returns>
        public MSPropertyBuilderInfo BuildDefaultPublicGetSetProperty(Type propertyType, string propertyName)
        {
            var fieldBuilder = BuildPrivateField(propertyType, MSNameHelper.NameOfBackingPrivateFieldForProperty(propertyName));
            var propertyBuilder = BuildPublicProperty(propertyType, propertyName);
            var methodGetBuilder = BuildDefaultGetMethodForProperty(propertyBuilder, fieldBuilder);
            var methodSetBuilder = BuildDefaultSetMethodForProperty(propertyBuilder, fieldBuilder);
            return new MSPropertyBuilderInfo(fieldBuilder, propertyBuilder, methodGetBuilder, methodSetBuilder);
        }

        /// <summary>
        /// Build proxy property for an virtual property in base class and add to given type.
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns>PropertyBuilder</returns>
        /// <remarks>
        /// All params must be not null; otherwise, exception is thrown.
        /// The property must be public, readwrite, virtual; otherwise, exception is thrown.
        /// </remarks>
        public PropertyBuilder BuildProxyPropertyForPropertyInfo(PropertyInfo propertyInfo)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyInfo, "propertyInfo");
            if (!MSPropertyHelper.IsPublicVirtualReadwriteNonAbstractProperty(propertyInfo))
            {
                throw new MSProxyException("Param propertyInfo must be public / virtual/ readrite");
            }

            return BuildPublicProperty(propertyInfo.PropertyType, propertyInfo.Name);
        }

        /// <summary>
        /// Build a setter method for a proxy property and set that setter method for the property.
        /// </summary>
        /// <param name="propertyBuilder">PropertyBuilder</param>
        /// <param name="backingPrivateFieldBuilder">Private backing field for that property</param>
        /// <returns>MethodBuilder</returns>
        /// <remarks>
        /// Since setter method for proxy property is not different than for normal property,
        /// this function actually calls BuildDefaultSetMethodForProperty function.
        /// </remarks>
        public MethodBuilder BuildProxySetMethodForProperty(PropertyBuilder propertyBuilder, FieldBuilder backingPrivateFieldBuilder, MethodBuilder flagPropertyBuilderSetMethod)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyBuilder, "propertyBuilder");
            MSParameterHelper.MakeSureInputParameterNotNull(backingPrivateFieldBuilder, "backingPrivateFieldBuilder");
            MSParameterHelper.MakeSureInputParameterNotNull(flagPropertyBuilderSetMethod, "flagPropertyBuilderSetMethod");

            // Method attributes
            var methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.Final // them
                | MethodAttributes.HideBySig;
            var methodBuilder = this.TypeBuilder.DefineMethod(
                MSNameHelper.NameOfSetMethodForProperty(propertyBuilder.Name),
                methodAttributes,
                typeof(void),
                new Type[] { propertyBuilder.PropertyType });
            // Parameter value
            var value = methodBuilder.DefineParameter(1, ParameterAttributes.None, "value");
            ILGenerator gen = methodBuilder.GetILGenerator();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0); // this
            gen.Emit(OpCodes.Ldc_I4_1); // true
            gen.Emit(OpCodes.Call, flagPropertyBuilderSetMethod); // Set IsLoaded = true

            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, backingPrivateFieldBuilder);

            gen.Emit(OpCodes.Ret);
            // Set this method to setter of property
            propertyBuilder.SetSetMethod(methodBuilder);
            // finished
            return methodBuilder;
        }

        /// <summary>
        /// Build a getter method for a proxy property and set that getter method for the property.
        /// </summary>
        /// <param name="propertyBuilder">PropertyBuilder</param>
        /// <param name="backingPrivateFieldBuilder">Backing private field for property</param>
        /// <param name="loadDataMethodInfo">Load data method for property</param>
        /// <returns>MethodBuilder</returns>
        /// <remarks>
        /// All params must be not null; otherwise, exception is thrown.
        /// This function checks if data is loaded already or not. If not, then load, otherwise, just return.
        /// </remarks>
        public MethodBuilder BuildProxyGetMethodForProperty(PropertyBuilder propertyBuilder, FieldBuilder backingPrivateFieldBuilder, MethodInfo loadDataMethodInfo)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyBuilder, "propertyBuilder");
            MSParameterHelper.MakeSureInputParameterNotNull(backingPrivateFieldBuilder, "backingPrivateFieldBuilder");
            MSParameterHelper.MakeSureInputParameterNotNull(loadDataMethodInfo, "loadDataMethodInfo");

            // Method attributes
            var methodAttributes = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.Final // them
                | MethodAttributes.HideBySig;
            var methodBuilder = this.TypeBuilder.DefineMethod(
                MSNameHelper.NameOfGetMethodForProperty(propertyBuilder.Name), 
                methodAttributes, 
                propertyBuilder.PropertyType, 
                new Type[] { });
            // Adding parameters
            ILGenerator gen = methodBuilder.GetILGenerator();
            // Preparing locals
            LocalBuilder local = gen.DeclareLocal(propertyBuilder.PropertyType);
            // Preparing labels
            Label label17 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, loadDataMethodInfo);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, backingPrivateFieldBuilder);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label17);
            gen.MarkLabel(label17);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // set getter method
            propertyBuilder.SetGetMethod(methodBuilder);
            // finished
            return methodBuilder;
        }
    }
}
