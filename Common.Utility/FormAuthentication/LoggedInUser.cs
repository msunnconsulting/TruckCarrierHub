namespace Common.Utility.FormAuthentication
{
    using Extensions;
    using System.Linq;

    /// <summary>
    /// This is a logged in user class.
    /// </summary>
    /// <typeparam name="RoleType">Role type of user</typeparam>
    /// <typeparam name="IdType">Id type of user</typeparam>
    public class LoggedInUser<RoleType, IdType>
    {
        // TODO : Validate RoleType & IdType

        /// <summary>
        /// Gets or sets a value indicating whether user checked remember me checkbox or not
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Gets or sets a value of the user Id type
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets a value of the user Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value of the user Email Address
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets a value of the user Roles
        /// </summary>
        public RoleType[] Roles { get; set; }

        /// <summary>
        /// This method checks that user has role or not
        /// </summary>
        /// <param name="role">Enter Role</param>
        /// <returns>return true or false for that user has role or not</returns>
        public bool HasRole(RoleType role)
        {
            return this.Roles.Contains(role);
        }

        /// <summary>
        /// This method checks that user has role or not
        /// </summary>
        /// <param name="role">Enter Role</param>
        /// <returns>return true or false for that user has role or not</returns>
        public bool HasRole(object role)
        {

            if (typeof(RoleType).IsEnum)
            {
                RoleType r = role.ToEnum<RoleType>();
                return this.Roles.Contains(r);
            }
            else
            {
                RoleType r = (RoleType)role;
                return this.Roles.Contains(r);
            }
        }

        /// <summary>
        /// This method checks that user has any role from given roles
        /// </summary>
        /// <param name="roles">Enter roles</param>
        /// <returns>indicates that user has role or not from given roles</returns>
        public bool HasRole(params RoleType[] roles)
        {
            foreach (RoleType role in roles)
                if (this.HasRole(role)) return true;

            return false;
        }

        /// <summary>
        /// This method checks that user has any role from given roles
        /// </summary>
        /// <param name="roles">Enter roles</param>
        /// <returns>indicates that user has role or not from given roles</returns>
        public bool HasRole(params object[] roles)
        {
            foreach (object role in roles)
                if (this.HasRole(role)) return true;

            return false;
        }
    }
}
