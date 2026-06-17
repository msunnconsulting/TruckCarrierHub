namespace Common.Utility.Reflection
{
    using System;
    using System.Reflection;

    /// <summary>
    /// this is a sealed Reflection Util class.
    /// </summary>
    public sealed class ReflectionUtil
    {
        /// <summary>
        /// This method is for check type of assembly
        /// </summary>
        /// <param name="strTypeName">Enter Name of assembly for which you want to get type</param>
        /// <returns>return type of assembly</returns>
        public static Type GetType(string strTypeName)
        {
            ////get all the items from this list
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            Type list = null;
            foreach (Assembly assembly in assemblies)
            {
                list = assembly.GetType(strTypeName, false, true);

                if (list != null)
                    break;
            }

            return list;
        }

        /// <summary>
        /// This is a method for execute method
        /// </summary>
        /// <param name="objectType">Enter object type for execute method</param>
        /// <param name="generickTypeArgName">Enter argument name of generic type</param>
        /// <param name="methodName">Enter Method name which will to be executed</param>
        /// <param name="methodGenerickTypeArgName">Enter argument name of generic method</param>
        /// <param name="methodArgs">Enter method arguments</param>
        /// <returns>returns method info</returns>
        public static object ExecuteMethod(Type objectType, string[] generickTypeArgName, string methodName, string[] methodGenerickTypeArgName, object[] methodArgs)
        {
            if (generickTypeArgName != null && generickTypeArgName.Length > 0)
            {
                Type[] typeArgs;
                typeArgs = new Type[generickTypeArgName.Length];
                for (int i = 0; i <= generickTypeArgName.Length - 1; i++)
                {
                    typeArgs[i] = GetType(generickTypeArgName[i]);
                }

                objectType = objectType.MakeGenericType(typeArgs);
            }

            object objGenericType = Activator.CreateInstance(objectType);

            MethodInfo genericMethod = objectType.GetMethod(methodName);

            if (methodGenerickTypeArgName != null && methodGenerickTypeArgName.Length > 0)
            {
                Type[] methodTypeArgs;
                methodTypeArgs = new Type[methodGenerickTypeArgName.Length];
                for (int i = 0; i <= methodGenerickTypeArgName.Length - 1; i++)
                {
                    methodTypeArgs[i] = GetType(methodGenerickTypeArgName[i]);
                }

                genericMethod = genericMethod.MakeGenericMethod(methodTypeArgs);
            }

            return genericMethod.Invoke(objGenericType, methodArgs);
        }
    }
}
