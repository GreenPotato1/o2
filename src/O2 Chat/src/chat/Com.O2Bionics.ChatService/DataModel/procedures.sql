CREATE PROCEDURE ADD_UPDATE_WIDGET_LOADS
(
  pCUSTOMER_ID IN WIDGET_LOAD.CUSTOMER_ID%TYPE
, pUPDATED IN WIDGET_LOAD.UPDATED%TYPE
, pINCREMENT IN WIDGET_LOAD.LOADS%TYPE
, pISOVERLOAD IN WIDGET_LOAD.ISOVERLOAD%TYPE DEFAULT NULL
, pLOADS OUT WIDGET_LOAD.LOADS%TYPE
, pISSET OUT WIDGET_LOAD.ISOVERLOAD%TYPE -- Whether the value has changed from 0 to 1.
)
AS 
  max_attempts NUMBER := 10;
  attempts NUMBER := 0;
  stamp WIDGET_LOAD.STAMP%TYPE := NULL;
  isOverRead WIDGET_LOAD.ISOVERLOAD%TYPE := NULL;
  isOver WIDGET_LOAD.ISOVERLOAD%TYPE := 0;
BEGIN
  pISSET := 0;
  IF (1 = pISOVERLOAD) THEN
    isOver := 1;
  END IF;

  <<main_loop>>
  LOOP
    attempts := attempts + 1;
    IF (max_attempts < attempts) THEN
      EXIT main_loop;
    END IF;
    
    BEGIN
      --Select the stamp.
      FOR curr IN (SELECT STAMP, ISOVERLOAD FROM WIDGET_LOAD WHERE CUSTOMER_ID = pCUSTOMER_ID AND UPDATED = pUPDATED) LOOP
        stamp := curr.STAMP;
        isOverRead := curr.ISOVERLOAD;
        EXIT;
      END LOOP;

      IF (stamp IS NULL) THEN -- Try to insert.
        BEGIN
          INSERT INTO WIDGET_LOAD(CUSTOMER_ID, UPDATED, LOADS, STAMP, ISOVERLOAD)
            VALUES (pCUSTOMER_ID, pUPDATED, pINCREMENT, 1, isOver);
          pLOADS := pINCREMENT;
          pISSET := isOver;
          RETURN; --Successful insert.

          EXCEPTION
          WHEN DUP_VAL_ON_INDEX THEN
            --Someone must have inserted.
            CONTINUE main_loop;
        END;
      END IF;
      
      BEGIN
        IF (1 = isOverRead) THEN --Once set to true, the value cannot be reset back.
          isOver := 1;
        END IF;

        UPDATE WIDGET_LOAD
        SET LOADS = LOADS + pINCREMENT, STAMP = STAMP + 1, ISOVERLOAD = isOver
        WHERE CUSTOMER_ID = pCUSTOMER_ID AND UPDATED = pUPDATED AND STAMP = stamp
        RETURNING LOADS INTO pLOADS;

        IF ((0 = isOverRead) AND (1 = isOver)) THEN --The value has changed from 0 to 1.
          pISSET := 1;
        END IF;

        RETURN;--Successful update.

        EXCEPTION
          WHEN NO_DATA_FOUND THEN
          --Someone must have updated.
          CONTINUE main_loop;
      END;
    END;
  END LOOP;
  
  --Faled to insert or update.
  RAISE_APPLICATION_ERROR(-20000, 'Failed to update the widget loads.');
END ADD_UPDATE_WIDGET_LOADS;
