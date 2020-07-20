using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService.DataModel
{
    public static class DatabaseObjectHelper
    {
        public static IEnumerable<USER_ROLE> CreateUserRoles(
            DateTime utcNow,
            CUSTOMER_USER user,
            bool isOwner,
            bool isAdmin,
            IEnumerable<DEPARTMENT> agentDepartments,
            IEnumerable<DEPARTMENT> supervisorDepartments)
        {
            if (isOwner)
                yield return UserRole(utcNow, user, UserRoleCode.Owner);
            if (isAdmin)
                yield return UserRole(utcNow, user, UserRoleCode.Admin);
            foreach (var dept in agentDepartments)
                yield return UserRole(utcNow, user, UserRoleCode.Agent, dept);
            foreach (var dept in supervisorDepartments)
                yield return UserRole(utcNow, user, UserRoleCode.Supervisor, dept);
        }

        public static CUSTOMER Customer(
            DateTime now,
            uint id,
            string name,
            string domains,
            ObjectStatus status = ObjectStatus.Active)
        {
            return new CUSTOMER
                {
                    ID = id,
                    STATUS_ID = (int)status,
                    CREATE_TIMESTAMP = now,
                    UPDATE_TIMESTAMP = now,
                    NAME = name,
                    DOMAINS = domains,
                };
        }

        public static CUSTOMER_USER User(
            DateTime now,
            uint id,
            CUSTOMER customer,
            string email,
            string password,
            string firstName,
            string lastName,
            ObjectStatus status = ObjectStatus.Active,
            bool isOnline = true)
        {
            return new CUSTOMER_USER
                {
                    ID = id,
                    CUSTOMER_ID = customer.ID,
                    EMAIL = email,
                    PASSWORD = password.ToPasswordHash(),
                    IS_ONLINE = (sbyte)(isOnline ? 1 : 0),
                    STATUS_ID = (int)status,
                    FIRST_NAME = firstName,
                    LAST_NAME = lastName,
                    CREATE_TIMESTAMP = now,
                    UPDATE_TIMESTAMP = now,
                };
        }

        public static USER_ROLE UserRole(
            DateTime utcNow,
            CUSTOMER_USER user,
            UserRoleCode roleCode,
            DEPARTMENT department = null)
        {
            return new USER_ROLE
                {
                    USER_ID = user.ID,
                    ADD_TIMESTAMP = utcNow,
                    ROLE_ID = (int)roleCode,
                    DEPARTMENT_ID = department?.ID,
                };
        }

        public static DEPARTMENT Department(
            DateTime utcNow,
            uint id,
            CUSTOMER customer,
            string name,
            string description,
            bool isPublic,
            ObjectStatus status = ObjectStatus.Active)
        {
            return new DEPARTMENT
                {
                    ID = id,
                    CUSTOMER_ID = customer.ID,
                    STATUS_ID = (int)status,
                    NAME = name,
                    DESCRIPTION = description,
                    IS_PUBLIC = isPublic ? 1 : 0,
                    ADD_TIMESTAMP = utcNow,
                    UPDATE_TIMESTAMP = utcNow,
                };
        }

        public static PROPERTY_BAG Property(
            CUSTOMER customer,
            string name,
            string value)
        {
            return new PROPERTY_BAG
                {
                    CUSTOMER_ID = customer.ID,
                    PROPERTY_FOLDER = "Chat",
                    PROPERTY_NAME = name,
                    PROPERTY_VALUE = value,
                };
        }
    }
}