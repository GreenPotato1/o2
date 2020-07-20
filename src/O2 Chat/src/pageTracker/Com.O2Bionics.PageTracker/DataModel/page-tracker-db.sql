SET FOREIGN_KEY_CHECKS=0; 

DROP TABLE IF EXISTS `ip_country`;
CREATE TABLE `ip_country` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ip_country_name_UNIQUE` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `ip_city`;
CREATE TABLE `ip_city` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ip_city_name_UNIQUE` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `ip_address`;
CREATE TABLE `ip_address` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `timestamp` datetime NOT NULL,
  `ip` varbinary(16) NOT NULL,
  `country_id` int(10) unsigned DEFAULT NULL,
  `city_id` int(10) unsigned DEFAULT NULL,
  `latitude` int(10) DEFAULT NULL,
  `longitude` int(10) DEFAULT NULL,
  `is_resolved` bool DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ip_address_ip_UNIQUE` (`ip`),
  CONSTRAINT `ip_address_city` FOREIGN KEY (`city_id`) REFERENCES `ip_city` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `ip_address_country` FOREIGN KEY (`country_id`) REFERENCES `ip_country` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `timezone`;
CREATE TABLE `timezone` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `offset` int(10) NOT NULL,
  `description` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `timezone_UNIQUE` (`offset`,`description`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `user_agent`;
CREATE TABLE `user_agent` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `md5hash` binary(16) NOT NULL,
  `device` varchar(64) NOT NULL,
  `os` varchar(64) NOT NULL,
  `agent` varchar(64) NOT NULL,
  `text` varchar(2048) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `user_agent_UNIQUE` (`md5hash`, `device`, `os`, `agent`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `site`;
CREATE TABLE `site` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `scheme` varchar(10) NOT NULL,
  `host` varchar(253) NOT NULL,
  `port` int(10) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `site_UNIQUE` (`scheme`, `host`, `port`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `visitor`;
CREATE TABLE `visitor` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `external_id` varchar(256) DEFAULT NULL,
  `customer_id` int(10) unsigned NOT NULL,
  `timestamp` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `page_history`;
CREATE TABLE `page_history` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `timestamp` datetime NOT NULL,
  `visitor_id` int(10) unsigned NOT NULL,
  `timezone_id` int(10) unsigned NOT NULL,
  `user_agent_id` int(10) unsigned NOT NULL,
  `ip_address_id` int(10) unsigned NOT NULL,
  `site_id` int(10) unsigned NOT NULL,
  `path` varchar(1024) NOT NULL,
  `query` varchar(2048) NOT NULL,
  `fragment` varchar(1024) NOT NULL,
  `custom_text` varchar(2048) DEFAULT NULL,
  PRIMARY KEY (`id`),
  CONSTRAINT `page_history_ip_address` FOREIGN KEY (`ip_address_id`) REFERENCES `ip_address` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `page_history_site` FOREIGN KEY (`site_id`) REFERENCES `site` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `page_history_timezone` FOREIGN KEY (`timezone_id`) REFERENCES `timezone` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `page_history_user_agent` FOREIGN KEY (`user_agent_id`) REFERENCES `user_agent` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `page_history_visitor` FOREIGN KEY (`visitor_id`) REFERENCES `visitor` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

SET FOREIGN_KEY_CHECKS=1;