Create Database ParkingSpot;

USE ParkingSpot;

CREATE TABLE Parking
(
ID int NOT NULL IDENTITY(1,1) PRIMARY KEY ,
TagNumber varchar(255) NOT NULL,
InTime DateTime,
OutTime DateTime,
ElapsedTime int,
 Fee int,
 IsParked bit NOT NULL default 0
);
--------------------------------------------------------------------------------------------------
Create procedure [dbo].[Getvailableslots] 
as  
DECLARE @Availablespotsqry varchar(100)
begin  
   Set @Availablespotsqry = 'select Count(*) as Availablespots from Parking where IsParked = 1';
    EXEC(@Availablespotsqry)
End
Exec [ParkingSpot].[dbo].[Getvailableslots]
--------------------------------------------------------------------------------------------------

CREATE procedure [dbo].[CheckIsCarParked] (@tagnumber Varchar(50))
as  

begin  
  SELECT count(*) as IsParked FROM Parking where IsParked = 1 and TagNumber =@tagnumber;
   
End

--Exec [ParkingSpot].[dbo].[CheckIsCarParked] 'okddf556'
--------------------------------------------------------------------------------------------------

CREATE procedure [dbo].[GetTakenParkingspots]
as  
begin  
  SELECT TagNumber, InTime, ElapsedTime,Fee FROM Parking where IsParked = 1;
   
End

--exec [ParkingSpot].[dbo].[GetTakenParkingspots]
--------------------------------------------------------------------------------------------------

CREATE procedure [dbo].[InParkingspot](@tagnumber Varchar(50),@defaultfeeperHr int )
as  
begin  
-- min elapsed time taking 1 as default & While parking making IsParked flag as 1
  insert into Parking(TagNumber, InTime,OutTime, ElapsedTime,Fee,IsParked) Values (@tagnumber,GetDate(),GetDate(),1,@defaultfeeperHr,1);
   
End

--exec [ParkingSpot].[dbo].[InParkingspot] 'Vino242'
--------------------------------------------------------------------------------------------------

Create procedure [dbo].[OutParkingspot](@tagnumber Varchar(50),@defaultfeeperHr int)
as  
DECLARE @fee int
begin  
  Update Parking SET OutTime = GetDate(),IsParked = 0 where TagNumber = @tagnumber
  Update Parking SET ElapsedTime = DATEDIFF(HOUR,(SELECT InTime from Parking where TagNumber = @tagnumber),(SELECT OutTime from Parking where TagNumber = @tagnumber))
  where TagNumber = @tagnumber

 
   Update Parking 
   SET Fee = ( Select CASE 
			WHEN ElapsedTime = 0 THEN @defaultfeeperHr
			WHEN ElapsedTime != 0 THEN (ElapsedTime * @defaultfeeperHr) END as Fee 
  from Parking where TagNumber = @tagnumber)
   where TagNumber = @tagnumber
End

--exec [ParkingSpot].[dbo].[OutParkingspot] 'okdf556'
--------------------------------------------------------------------------------------------------

CREATE procedure [dbo].[CheckVehicleParkedOrLeft] (@tagnumber Varchar(50))
as  

begin  
  SELECT count(*) as IsParked FROM Parking where IsParked = 1 and TagNumber =@tagnumber;
   
End
--------------------------------------------------------------------------------------------------
Create procedure [dbo].[CalculatePakingstats] (@defaultfeeperHr int)
as  
DECLARE @todayRevenue int
DECLARE @Availablespots int
DECLARE @AvgCarsPerDay int
DECLARE @AVGRevenuePerDay int
DECLARE @subquerycount int
begin  
	SET @Availablespots = @defaultfeeperHr - (SELECT COUNT(*) from Parking where IsParked = 1)              
	SET @todayRevenue = (SELECT Case 
								WHEN (SELECT count(Fee) FROM Parking where  cast(InTime as date) = cast(GETDATE() as date)) > 0 Then (SELECT Sum(Fee) FROM Parking where  cast(InTime as date) = cast(GETDATE() as date))
								ELSE 0
								END as todayRevenue)
	SET @AVGRevenuePerDay = (SELECT AVG(Fee) FROM Parking where DATEDIFF(DAY,InTime,GETDATE()) <30 )
	--SET @subquerycount = (SELECT COUNT(*) AS num_cars, cast(InTime as date) AS park_date FROM Parking  GROUP BY InTime)
	SET @AvgCarsPerDay = (SELECT AVG(num_cars) FROM (SELECT COUNT(TagNumber) AS num_cars FROM Parking  GROUP BY cast(InTime as date)) as dailyCars)
	
select @Availablespots as Availablespots, @todayRevenue as todayRevenue,@AvgCarsPerDay as AvgCarsPerDay,@AVGRevenuePerDay as AVGRevenuePerDay
End

--EXEC [dbo].[CalculatePakingstats]
--------------------------------------------------------------------------------------------------

CREATE procedure [dbo].[CheckValidCarParked] (@tagnumber Varchar(50))
as  

begin  
  SELECT count(*) as IsCarExist FROM Parking where TagNumber =@tagnumber;
   
End

--------------------------------------------------------------------------------------------------
