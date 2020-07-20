using Com.O2Bionics.FeatureService.Impl.DataModel;
using log4net;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.FeatureService.Impl.Legacy
{
    public class FeaturesManager
    {
        public static readonly string SHARE_DESKTOP = "communicator.app.desktop.share";
        public static readonly string SHARE_DESKTOP_CORPORATE = "communicator.app.desktop.share.corporate";
        public static readonly string MONITOR_CALLS = "monitor_calls";
        public static readonly string LIMIT_SUBACCOUNTS_MAX = "limit.subaccounts.max";
        public static readonly string SWITCHBOARD_ADDON = "communicator.app.switchboard";
        public static readonly string SWITCHBOARD_MONITOR = "communicator.app.switchboard.monitor";
        public static readonly string SWITCHBOARD_HANGUP = "communicator.app.switchboard.hangup";

        private static readonly ILog m_logger = LogManager.GetLogger(typeof(FeaturesManager));


        private readonly DatabaseFactory m_databaseFactory;

        public FeaturesManager(DatabaseFactory databaseFactory)
        {
            m_databaseFactory = databaseFactory;
        }

        public string GetFeatureForUserIdFromCustomerOrActiveAddOnOrService(int userid, int serviceid, string featureCode)
        {
            //check user overwride
            var value = GetCustomerFeatureByCode(userid, featureCode);
            if (value != null) return value;

            // check addon
            value = GetFeatureFromActiveAddOnForUserIdByFeatureCode(userid, featureCode);
            if (value != null) return value;

            //get from service
            value = GetServiceFeatureForServiceIdByCode(serviceid, featureCode);
            return value;
        }

        private const string GetActiveAddOnFeatureForUserByFeatureCodeSql =
            @"  select 
                    * 
                from 
                    ( 
                        select 
                            s.service_id, 
                            GetServiceFeatureUser(s.service_id, :userid, :feature_code) as feature_value 
                        from 
                            services s, 
                            service_subscription ss 
                        where 
                            ss.service_id = s.service_id 
                            and ss.userid = :userid 
                            and s.service_type_id > 0 
                            and nvl(ss.is_deleted, 0) = 0 
                    ) 
                where 
                    feature_value is not null";

        public string GetFeatureFromActiveAddOnForUserIdByFeatureCode(int userid, string featureCode)
        {
            return m_databaseFactory.Query(
                "prductCode",
                database =>
                    {
                        using (var cmd = new OracleCommand(GetActiveAddOnFeatureForUserByFeatureCodeSql))
                        {
                            cmd.Parameters.Add("userid", OracleDbType.Int32).Value = userid;
                            cmd.Parameters.Add("feature_code", OracleDbType.Varchar2).Value = featureCode;
                            using (var reader = database.ExecuteReader(cmd))
                            {
                                if (reader.Read())
                                {
                                    return OracleHelper.GetStringNull(reader, "feature_value");
                                }
                            }
                            return null;
                        }
                    });
        }


        private const string GetCustomerFeatureByCodeSql =
            @"  select 
                    cf.feature_value,
                    cf.required_service_id,
                    cf.MINIMUM_REQUIRED_PLANID 
                from 
                    features f, 
                    customer_features cf 
                where 
                    f.feature_id = cf.feature_id 
                    and f.feature_code = :feature_code 
                    and cf.userid = :userid 
                    and nvl(cf.expires_on, sysdate+1) > sysdate";

        public string GetCustomerFeatureByCode(int userid, string featureCode)
        {
            string featureValue = null;
            return m_databaseFactory.Query(
                "prductCode",
                database =>
                    {
                        using (var cmd = new OracleCommand(GetCustomerFeatureByCodeSql))
                        {
                            cmd.Parameters.Add("feature_code", OracleDbType.Varchar2).Value = featureCode;
                            cmd.Parameters.Add("userid", OracleDbType.Int32).Value = userid;
                            using (var reader = database.ExecuteReader(cmd))
                            {
                                if (reader.Read())
                                {
                                    var requiredServiceId = OracleHelper.GetInt32Null(reader, "required_service_id");
                                    var minimumRequiredPlanId = OracleHelper.GetInt32Null(reader, "MINIMUM_REQUIRED_PLANID");

                                    // TODO
                                    // plan === service
                                    // required service id
                                    // if feature value != null, check if has service_id
                                    // 

                                    featureValue = OracleHelper.GetStringNull(reader, "feature_value");
                                    if (requiredServiceId > 0)
                                    {
                                        //we have a condition on the service been active for the user
                                        if (IsServiceActiveByServiceId(userid, requiredServiceId))
                                        {
                                            //ok
                                        }
                                        else
                                        {
                                            featureValue = null;
                                        }
                                    }
                                }
                            }
                        }

                        return featureValue;
                    });
        }


        private const string GetServiceCountSql =
            @"  select 
                    count(*) 
                from 
                    services s, 
                    service_subscription ss 
                where 
                    ss.service_id=s.service_id 
                    and ss.userid=:userid 
                    and s.service_id=:serviceid 
                    and nvl(ss.is_deleted,0)=0";

        public bool IsServiceActiveByServiceId(int userid, int serviceid)
        {
            return m_databaseFactory.Query(
                "prductCode",
                database =>
                    {
                        using (var cmd = new OracleCommand(GetServiceCountSql))
                        {
                            cmd.Parameters.Add("userid", OracleDbType.Int32).Value = userid;
                            cmd.Parameters.Add("serviceid", OracleDbType.Int32).Value = serviceid;
                            if (OracleHelper.DbIntValue(database.ExecuteScalar(cmd)) > 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    });
        }

        private const string GetServiceFeatureForServiceIdByCodeSql =
            @"  select 
                    feature_value 
                from 
                    service_features sf,
                    features f 
                where 
                    sf.feature_id=f.feature_id 
                    and sf.service_id=:serviceid 
                    and f.feature_code=:feature_code";

        public string GetServiceFeatureForServiceIdByCode(int serviceid, string featureCode)
        {
            return m_databaseFactory.Query(
                "prductCode",
                database =>
                    {
                        using (var cmd = new OracleCommand(GetServiceFeatureForServiceIdByCodeSql))
                        {
                            cmd.Parameters.Add("serviceid", OracleDbType.Int32).Value = serviceid;
                            cmd.Parameters.Add("feature_code", OracleDbType.Varchar2).Value = featureCode;
                            using (var reader = database.ExecuteReader(cmd))
                            {
                                while (reader.Read())
                                {
                                    return OracleHelper.GetStringNull(reader, 0);
                                }
                            }
                        }

                        return null;
                    });
        }

        private const string GetServiceValueByTypeIdSql =
            @"  select 
                    nvl(sum(s.term*ss.quantity),0)
                from 
                    services s, 
                    service_subscription ss 
                where 
                    ss.service_id=s.service_id 
                    and ss.userid=:userid 
                    and s.service_type_id=:service_type_id 
                    and nvl(ss.is_deleted,0)=0";

        public int GetServiceValueByTypeId(int userid, int serviceTypeId)
        {
            return m_databaseFactory.Query(
                "prductCode",
                database =>
                    {
                        using (var cmd = new OracleCommand(GetServiceValueByTypeIdSql))
                        {
                            cmd.Parameters.Add("userid", OracleDbType.Int32).Value = userid;
                            cmd.Parameters.Add("service_type_id", OracleDbType.Int32).Value = serviceTypeId;
                            return OracleHelper.DbIntValue(database.ExecuteScalar(cmd));
                        }
                    });
        }
    }
}