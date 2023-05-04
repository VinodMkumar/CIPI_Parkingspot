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

Create procedure [dbo].[Getvailableslots] 
as  
DECLARE @Availablespotsqry varchar(100)
begin  
   Set @Availablespotsqry = 'select Count(*) as Availablespots from Parking where IsParked = 1';
    EXEC(@Availablespotsqry)
End
Exec [ParkingSpot].[dbo].[Getvailableslots]

CREATE procedure [dbo].[CheckIsCarParked] (@tagnumber Varchar(50))
as  

begin  
  SELECT count(*) as IsParked FROM Parking where IsParked = 1 and TagNumber =@tagnumber;
   
End

--Exec [ParkingSpot].[dbo].[CheckIsCarParked] 'okddf556'

CREATE procedure [dbo].[GetTakenParkingspots]
as  
begin  
  SELECT TagNumber, InTime, ElapsedTime,Fee FROM Parking where IsParked = 1;
   
End

--exec [ParkingSpot].[dbo].[GetTakenParkingspots]

CREATE procedure [dbo].[InParkingspot](@tagnumber Varchar(50))
as  
begin  
  insert into Parking(TagNumber, InTime,OutTime, ElapsedTime,Fee,IsParked) Values (@tagnumber,GetDate(),GetDate(),1,15,1);
   
End

--exec [ParkingSpot].[dbo].[InParkingspot] 'Vino242'

Create procedure [dbo].[OutParkingspot](@tagnumber Varchar(50))
as  
DECLARE @fee int
begin  
  Update Parking SET OutTime = GetDate(),IsParked = 0 where TagNumber = @tagnumber
  Update Parking SET ElapsedTime = DATEDIFF(HOUR,(SELECT InTime from Parking where TagNumber = @tagnumber),(SELECT OutTime from Parking where TagNumber = @tagnumber))
  where TagNumber = @tagnumber

 
   Update Parking 
   SET Fee = ( Select CASE 
			WHEN ElapsedTime = 0 THEN 15
			WHEN ElapsedTime != 0 THEN (ElapsedTime * 15) END as Fee 
  from Parking where TagNumber = @tagnumber)
   where TagNumber = @tagnumber
End

--exec [ParkingSpot].[dbo].[OutParkingspot] 'TDG356Y'

CREATE procedure [dbo].[CheckIsCarParked] (@tagnumber Varchar(50))
as  

begin  
  SELECT count(*) as IsParked FROM Parking where IsParked = 1 and TagNumber =@tagnumber;
   
End
ALTER procedure [dbo].[CalculatePakingstats] 
as  
DECLARE @todayRevenue int
DECLARE @Availablespots int
DECLARE @AvgCarsPerDay int
DECLARE @AVGRevenuePerDay int
DECLARE @subquerycount int
begin  
	SET @Availablespots = 15 - (SELECT COUNT(*) from Parking where IsParked = 1)              
	SET @todayRevenue = (SELECT Sum(Fee) FROM Parking where  cast(InTime as date) = cast(GETDATE() as date))
	SET @AVGRevenuePerDay = (SELECT AVG(Fee) FROM Parking where DATEDIFF(DAY,InTime,GETDATE()) <30 )
	--SET @subquerycount = (SELECT COUNT(*) AS num_cars, cast(InTime as date) AS park_date FROM Parking  GROUP BY InTime)
	SET @AvgCarsPerDay = (SELECT AVG(num_cars) FROM (SELECT COUNT(TagNumber) AS num_cars FROM Parking  GROUP BY cast(InTime as date)) as dailyCars)
	
select @Availablespots as Availablespots, @todayRevenue as todayRevenue,@AvgCarsPerDay as AvgCarsPerDay,@AVGRevenuePerDay as AVGRevenuePerDay
End

--EXEC [dbo].[CalculatePakingstats]

