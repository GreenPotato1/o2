CREATE PROCEDURE `add_record`(
	in timestampUtc datetime,
    in ipAddressId int unsigned,
    in ip varbinary(16),
    in ipCountryName varchar(128) character set utf8,
    in ipCityName varchar(128) character set utf8,
    in ipLatitude int,
    in ipLongitude int,
    in customerId int unsigned,
    in visitorId int unsigned,
    in visitorExternalId varchar(256) character set utf8,
    in timezoneOffset int,
    in timezoneDescription varchar(128) character set utf8,
    in userAgentText varchar(2048) character set utf8,
    in userAgentDevice varchar(64) character set utf8,
    in userAgentOs varchar(64) character set utf8,
    in userAgentAgent varchar(64) character set utf8,
    in siteScheme varchar(10),
    in siteHost varchar(253) character set utf8,
    in sitePort int,
    in urlPath varchar(1024) character set utf8,
	in urlQuery varchar(2048) character set utf8,
	in urlFragment varchar(1024) character set utf8,
	in customText varchar(2048) character set utf8,
	in updateIpLocationFlag bool,
	out pageHistoryId int unsigned,
	out visitorId2 int unsigned
	)
BEGIN
	declare cityId int unsigned;
    declare timezoneId int unsigned;
    declare userAgentId int unsigned;
    declare userAgentMd5Hash binary(16);
    declare siteId int unsigned;
    declare ipCountryId int unsigned;
    declare ipCityId int unsigned;

	if (isnull(ipAddressId) || updateIpLocationFlag) then
		set ipCountryId := null;
		if (not isnull(ipCountryName)) then
			set ipCountryId := (select `id` from `ip_country` where `name` = ipCountryName);
			if (isnull(ipCountryId)) then
				insert into `ip_country` (`name`) values (ipCountryName);
				set ipCountryId = last_insert_id();
			end if;
		end if;
		set ipCityId := null;
		if (not isnull(ipCityName)) then
			set ipCityId := (select `id` from `ip_city` where `name` = ipCityName);
			if (isnull(ipCityId)) then
				insert into `ip_city` (`name`) values (ipCityName);
				set ipCityId = last_insert_id();
			end if;
		end if;
		if (updateIpLocationFlag && not isnull(ipAddressId)) then
			update ip_address 
				set
					`timestamp` = timestampUtc,
					`country_id` = ipCountryId,
					`city_id` = ipCityId,
					`latitude` = ipLatitude,
					`longitude` = ipLongitude,
					`is_resolved` = 1
				where `id` = ipAddressId;
		else
			insert into ip_address (`timestamp`, `ip`, `country_id`, `city_id`, `latitude`, `longitude`, `is_resolved`)
				values (timestampUtc, ip, ipCountryId, ipCityId, ipLatitude, ipLongitude, updateIpLocationFlag);
			set ipAddressId := last_insert_id();
		end if;
	end if;

    set visitorId2 := (select `id` from `visitor` where `id` = visitorId);
    if (isnull(visitorId2)) then
		insert into `visitor` (`external_id`, `customer_id`, `timestamp`)
			values (visitorExternalId, customerId, timestampUtc);
        set visitorId2 = last_insert_id();
	else
        if (not isnull(visitorExternalId) && 0 < length(visitorExternalId)) then
		    update `visitor` set `external_id` = visitorExternalId where `id` = visitorId2;
	    end if;
	end if;

    set timezoneId := (select `id` from `timezone` where `offset` = timezoneOffset and `description` = timezoneDescription);
    if (isnull(timezoneId)) then
		insert into `timezone` (`offset`, `description`)
			values (timezoneOffset, timezoneDescription);
		set timezoneId = last_insert_id();
    end if;

    set userAgentMd5Hash := unhex(md5(userAgentText));
    set userAgentId := (select `id` from `user_agent`
							where `md5hash` = userAgentMd5Hash and device = userAgentDevice and os = userAgentOs and agent = userAgentAgent);
    if (isnull(userAgentId)) then
		insert into `user_agent` (`md5hash`, `text`, `device`, `os`, `agent`)
			values (userAgentMd5Hash, userAgentText, userAgentDevice, userAgentOs, userAgentAgent);
        set userAgentId = last_insert_id();
	end if;

    set siteId := (select `id` from `site` where `scheme` = siteScheme and `host` = siteHost and `port` = sitePort);
    if (isnull(siteId)) then
		insert into `site` (`scheme`, `host`, `port`)
			values (siteScheme, siteHost, sitePort);
        set siteId = last_insert_id();
	end if;

    insert into `page_history` (
		`timestamp`,
        `visitor_id`,
        `timezone_id`,
        `user_agent_id`,
        `ip_address_id`,
        `site_id`,
        `path`,
        `query`,
        `fragment`,
        `custom_text`
        ) values (
        timestampUtc,
        visitorId2,
        timezoneId,
        userAgentId,
        ipAddressId,
        siteId,
        urlPath,
        urlQuery,
        urlFragment,
        customText
        );
	set pageHistoryId = last_insert_id();
END