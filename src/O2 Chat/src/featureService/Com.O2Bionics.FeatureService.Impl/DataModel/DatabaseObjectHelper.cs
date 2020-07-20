using System;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.FeatureService.Impl.DataModel
{
    public class DatabaseObjectHelper
    {
        private readonly Database m_db;

        private int m_customerId = 1;
        private int m_featureId = 1;
        private int m_serviceId = 1;

        public DatabaseObjectHelper(Database db)
        {
            m_db = db;
        }

        public int AddCustomer(string name, int? customerId = null)
        {
            var id = customerId ?? m_customerId++;
            const string sql = "insert into CUSTOMER (USERID, NAME) values (:id, :name)";
            using (var cmd = new OracleCommand(sql))
            {
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                cmd.Parameters.Add("name", OracleDbType.Varchar2).Value = name;

                m_db.Execute(cmd);
            }
            return id;
        }

        public int AddFeature(string code, FeatureValueAggregationMethod? aggregationMethod = null, bool? useSubscriptionQuantity = null)
        {
            var id = m_featureId++;
            const string sql =
                @"insert into FEATURES 
                        (FEATURE_ID, FEATURE_CODE, FEATURE_TYPEID, AGGREGATION_METHOD_ID, USE_SUBSCRIPTION_QUANTITY) 
                    values 
                        (:id, :code, 0, :agm, :usq)";
            using (var cmd = new OracleCommand(sql))
            {
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                cmd.Parameters.Add("code", OracleDbType.Varchar2).Value = code;
                cmd.Parameters.Add("agm", OracleDbType.Int32).Value = aggregationMethod == null
                    ? (object)DBNull.Value
                    : (int)aggregationMethod.Value;
                cmd.Parameters.Add("usq", OracleDbType.Int32).Value = useSubscriptionQuantity == null
                    ? (object)DBNull.Value
                    : (useSubscriptionQuantity.Value ? 1 : 0);

                m_db.Execute(cmd);
            }
            return id;
        }

        public int AddService(string name, int? serviceTypeId = null)
        {
            var id = m_serviceId++;

            const string sql =
                @"insert into SERVICES 
                        (SERVICE_ID, SERVICE_NAME, SERVICE_TYPE_ID, IS_MULTIPLE) 
                    values 
                        (:id, :name, :typeId, 0)";
            using (var cmd = new OracleCommand(sql))
            {
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                cmd.Parameters.Add("name", OracleDbType.Varchar2).Value = name;
                cmd.Parameters.Add("serviceTypeId", OracleDbType.Int32).Value = serviceTypeId ?? (object)DBNull.Value;

                m_db.Execute(cmd);
            }
            return id;
        }

        public int AddServiceSubscription(int serviceId, int userId, int? quantity = null, bool? isDeleted = null)
        {
            const string sql =
                @"  insert into SERVICE_SUBSCRIPTION 
                            (SUBSCRIPTION_SKEY, SERVICE_ID, USERID, AMOUNT, QUANTITY, IS_DELETED) 
                        values 
                            (SEQ_SERVICE_SUBSCRIPTION.NextVal, :serviceId, :userId, 99, :quantity, :isDeleted)";
            using (var cmd = new OracleCommand(sql))
            {
                cmd.Parameters.Add("serviceId", OracleDbType.Int32).Value = serviceId;
                cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("quantity", OracleDbType.Int32).Value = quantity ?? (object)DBNull.Value;
                cmd.Parameters.Add("isDeleted", OracleDbType.Int32).Value = isDeleted == null
                    ? (object)DBNull.Value
                    : (isDeleted.Value ? 1 : 0);

                m_db.Execute(cmd);
            }
            using (var cmd2 = new OracleCommand("select SEQ_SERVICE_SUBSCRIPTION.CurrVal from dual"))
            {
                return Convert.ToInt32(m_db.ExecuteScalar(cmd2));
            }
        }

        public void AddCustomerFeatureValue(int userId, int featureId, string featureValue, DateTime? expiresOn, int? requiredServiceId = null)
        {
            const string sql =
                @"insert into CUSTOMER_FEATURES 
                        (USERID, FEATURE_ID, FEATURE_VALUE, REQUIRED_SERVICE_ID, EXPIRES_ON) 
                    values 
                        (:userId, :featureId, :featureValue, :requiredServiceId, :expiresOn)";
            using (var cmd = new OracleCommand(sql))
            {
                cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("featureId", OracleDbType.Int32).Value = featureId;
                cmd.Parameters.Add("featureValue", OracleDbType.Varchar2).Value = featureValue;
                cmd.Parameters.Add("requiredServiceId", OracleDbType.Int32).Value = requiredServiceId ?? (object)DBNull.Value;
                cmd.Parameters.Add("expiresOn", OracleDbType.Date).Value = expiresOn ?? (object)DBNull.Value;

                m_db.Execute(cmd);
            }
        }

        public void AddServiceFeatureValue(int serviceId, int featureId, string featureValue)
        {
            const string sql =
                @"insert into SERVICE_FEATURES 
                        (SERVICE_ID, FEATURE_ID, FEATURE_VALUE) 
                    values 
                        (:serviceId, :featureId, :featureValue)";
            using (var cmd = new OracleCommand(sql))
            {
                cmd.Parameters.Add("serviceId", OracleDbType.Int32).Value = serviceId;
                cmd.Parameters.Add("featureId", OracleDbType.Int32).Value = featureId;
                cmd.Parameters.Add("featureValue", OracleDbType.Varchar2).Value = featureValue;

                m_db.Execute(cmd);
            }
        }

        public int AddCasFeature(string featureCode)
        {
            return AddFeature(featureCode, FeatureValueAggregationMethod.CAS, false);
        }
    }
}