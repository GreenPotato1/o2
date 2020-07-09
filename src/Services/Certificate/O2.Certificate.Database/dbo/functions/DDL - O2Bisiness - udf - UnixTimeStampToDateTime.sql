/* 
* Project: O2.Business 
* Function: udf_UnixTimeStampToDateTime 
* Autor: Denis Prokhorchik
* Date: 18-05-2019
*/

if (object_id('dbo.udf_UnixTimeStampToDateTime') is not null)
	drop function dbo.udf_UnixTimeStampToDateTime;
go


CREATE FUNCTION dbo.udf_UnixTimeStampToDateTime (
@ctimestamp bigint
)
RETURNS datetime
AS 
BEGIN
  /* Function body */
  declare @return datetime
  SELECT @return = DATEADD(SECOND, @ctimestamp, {d '1970-01-01'})
  return @return
END