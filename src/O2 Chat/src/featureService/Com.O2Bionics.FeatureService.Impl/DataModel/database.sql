
CREATE TABLE CUSTOMER
(
	USERID						NUMBER			NOT NULL,
	NAME						VARCHAR2(256)	NOT NULL
);
ALTER TABLE CUSTOMER ADD CONSTRAINT PK_CUSTOMER PRIMARY KEY (USERID);

CREATE TABLE FEATURE_AGGREGATION_METHOD
(
	AGGREGATION_METHOD_ID		NUMBER			NOT NULL,
	DESCRIPTION					VARCHAR2(20)	NOT NULL
);
ALTER TABLE FEATURE_AGGREGATION_METHOD ADD CONSTRAINT PK_FT_AGGR_METHOD PRIMARY KEY (AGGREGATION_METHOD_ID);
INSERT INTO FEATURE_AGGREGATION_METHOD VALUES (0, 'CAS'); 
INSERT INTO FEATURE_AGGREGATION_METHOD VALUES (1, 'Sum');
INSERT INTO FEATURE_AGGREGATION_METHOD VALUES (2, 'Min');
INSERT INTO FEATURE_AGGREGATION_METHOD VALUES (3, 'Max');

-- description
CREATE TABLE FEATURES
(
	FEATURE_ID					NUMBER			NOT NULL,
	FEATURE_CODE				VARCHAR2(256)	NOT NULL,
	FEATURE_TYPEID				NUMBER			NOT NULL,
	NOTES						VARCHAR2(128)	NULL,
	FEATURE_NAME				VARCHAR2(64)	NULL,
	IS_SHOW_FOR_RESELLER		NUMBER(1)		NULL,
	AGGREGATION_METHOD_ID		NUMBER			NULL,
	USE_SUBSCRIPTION_QUANTITY	NUMBER			NULL
);
ALTER TABLE FEATURES ADD CONSTRAINT PK_FEATURES PRIMARY KEY (FEATURE_ID);
ALTER TABLE FEATURES ADD CONSTRAINT UQ_FEATURES UNIQUE (FEATURE_CODE);
ALTER TABLE FEATURES ADD CONSTRAINT FK_FEATURES_AM FOREIGN KEY (AGGREGATION_METHOD_ID) REFERENCES FEATURE_AGGREGATION_METHOD (AGGREGATION_METHOD_ID);

-- a package of features
CREATE TABLE SERVICES
(
	SERVICE_ID					NUMBER			NOT NULL,
	SERVICE_NAME				VARCHAR2(128)	NULL,
	SERVICE_DESCRIPTION			VARCHAR2(1024)	NULL,
	TRANSLATION_CODE			VARCHAR2(96)	NULL,
	SERVICE_TYPE_ID				NUMBER			NULL,
	TERM						NUMBER			NULL,
	IS_MULTIPLE					NUMBER			NOT NULL,
	UNIT						VARCHAR2(100)	NULL,
	MEMO						VARCHAR2(2000)	NULL,
	REQUIRED_FEATURE_CODE		VARCHAR2(256)	NULL,
	QUANTITY_TEXT				VARCHAR2(128)	NULL,
	IS_SHOW_ON_MAIN				NUMBER(1)		NULL,
	IS_DELETED					NUMBER(1)		NULL,
	IS_INTERNAL					NUMBER			NULL,
	IS_NO_REFUND				NUMBER(1)		NULL,
	GROUP_NAME					VARCHAR2(64)	NULL,
	IS_RESELLER_PACKAGE			NUMBER(1)		NULL,
	RESELLER_USERID				NUMBER			NULL,
	IS_ACTIVE					NUMBER(1)		NULL,
	BILLING_TYPE_ID				NUMBER(1)		NULL,
	COST						NUMBER			NULL
);
ALTER TABLE SERVICES ADD CONSTRAINT PK_SERVICES PRIMARY KEY (SERVICE_ID);


CREATE TABLE SERVICE_SUBSCRIPTION
(
	SUBSCRIPTION_SKEY			NUMBER			NOT NULL,
	EFFECTIVE_DATE				DATE			NULL,
	COMMENTS					NVARCHAR2(768)	NULL,
	AMOUNT						NUMBER			NOT NULL,
	OPERATION_SKEY				NUMBER			NULL,
	TO_DATE						DATE			NULL,
	USERID						NUMBER			NULL,
	SERVICE_ID					NUMBER			NOT NULL,
	QUANTITY					NUMBER			NULL,
	IS_DELETED					NUMBER			NULL,
	BILLING_INTERVAL			NUMBER			NULL,
	CANCEL_DATE					DATE			NULL
);
CREATE SEQUENCE SEQ_SERVICE_SUBSCRIPTION INCREMENT BY 1 START WITH 1;
ALTER TABLE SERVICE_SUBSCRIPTION ADD CONSTRAINT PK_SERVICE_SUBSCR PRIMARY KEY (SUBSCRIPTION_SKEY);
ALTER TABLE SERVICE_SUBSCRIPTION ADD CONSTRAINT FK_SERVICE_SUBSCR_USER FOREIGN KEY (USERID) REFERENCES CUSTOMER (USERID);
ALTER TABLE SERVICE_SUBSCRIPTION ADD CONSTRAINT FK_SERVICE_SUBSCR_SERVICE FOREIGN KEY (SERVICE_ID) REFERENCES SERVICES (SERVICE_ID);
CREATE INDEX IDX_SERVICE_SB ON SERVICE_SUBSCRIPTION (USERID, SERVICE_ID);

CREATE TABLE SERVICE_FEATURES
(
	FEATURE_VALUE				VARCHAR2(1024)	NULL,
	SERVICE_ID					NUMBER			NOT NULL,
	FEATURE_ID					NUMBER			NULL,
	USERID						NUMBER			NULL
);
ALTER TABLE SERVICE_FEATURES ADD CONSTRAINT FK_SERVICE_FT_SERVICE FOREIGN KEY (SERVICE_ID) REFERENCES SERVICES (SERVICE_ID);
ALTER TABLE SERVICE_FEATURES ADD CONSTRAINT FK_SERVICE_FT_FEATURE FOREIGN KEY (FEATURE_ID) REFERENCES FEATURES (FEATURE_ID);
ALTER TABLE SERVICE_FEATURES ADD CONSTRAINT FK_SERVICE_FT_CUSTOMER FOREIGN KEY (USERID) REFERENCES CUSTOMER (USERID);
CREATE INDEX IDX_SERVICE_FT ON SERVICE_FEATURES (FEATURE_ID, SERVICE_ID);


CREATE TABLE CUSTOMER_FEATURES
(
	USERID						NUMBER			NOT NULL,
	FEATURE_ID					NUMBER			NOT NULL,
	FEATURE_VALUE				VARCHAR2(256)	NULL,
	MINIMUM_REQUIRED_PLANID		NUMBER			NULL,
	REQUIRED_SERVICE_ID			NUMBER			NULL,
	EXPIRES_ON					DATE			NULL
);
ALTER TABLE CUSTOMER_FEATURES ADD CONSTRAINT FK_CUSTOMER_FEATURES_CUSTOMER FOREIGN KEY (USERID) REFERENCES CUSTOMER (USERID);
ALTER TABLE CUSTOMER_FEATURES ADD CONSTRAINT FK_CUSTOMER_FEATURES_FEATURE FOREIGN KEY (FEATURE_ID) REFERENCES FEATURES (FEATURE_ID);
ALTER TABLE CUSTOMER_FEATURES ADD CONSTRAINT FK_CUSTOMER_FEATURES_REQSERV FOREIGN KEY (REQUIRED_SERVICE_ID) REFERENCES SERVICES (SERVICE_ID);
CREATE INDEX IDX_CUSTOMER_FT ON CUSTOMER_FEATURES (USERID, FEATURE_ID, EXPIRES_ON);