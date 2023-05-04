namespace ParkingspotProject.Models
{
    public class Parkingspot
    {
//        public int Id { get; set; }
        public string TagNumber { get; set; }
        public DateTime InTime { get; set; }
        //public DateTime OutTime { get; set; }
        public long Fee { get; set; }
        public int ElapsedTime { get; set; }
        //public bool IsParked { get; set; }
    }
    public class ParkingfeeData
    {
        public string defaultFeeperHour { get; set; }
        public string totalSpotsAvailable { get; set; }

    }
    public class ParkingModalStats
    {
        public int Availablespots { get; set; }
        public int todayRevenue { get; set; }
        public int AvgCarsPerDay { get; set; }
        public int AVGRevenuePerDay { get; set; }

    }
    public class GetSpots
    {     
        public GetSpots(int availablespots, int TakenspotsCount, List<Parkingspot> parkingspots)
        {
            this.availablespotsCount = availablespots;
            this.TakenspotsCount = TakenspotsCount;
            this.parkingspotslist = parkingspots;
        }

        public int availablespotsCount { get; set; }
        public int TakenspotsCount { get; set; }
        public List<Parkingspot> parkingspotslist { get; set; }
    }
}
