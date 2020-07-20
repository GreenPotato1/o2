CREATE OR REPLACE FUNCTION "GETSERVICEFEATUREUSER" (
		in_service_id in number,
		in_userid in number,
		in_feature_code in varchar2)
	return varchar2 is
		
		feature_value varchar2(64);
		CURSOR c1 IS
			select 
				sf.feature_value 
			from 
				service_features sf, 
				features f 
			where 
				sf.service_id = in_service_id 
				and f.feature_id = sf.feature_id
				and f.feature_code = in_feature_code
				and (sf.userid = in_userid or sf.userid is null)
			order by 
				nvl(sf.userid, -1) desc;
		
		begin
			open c1;
			fetch c1 into feature_value;
			close c1;
			return feature_value;
		end;