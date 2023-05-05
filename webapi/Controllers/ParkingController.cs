using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ParkingspotProject.Models;
using System.Collections.Generic;
using System.Data;

namespace ParkingspotProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParkingController : ControllerBase
    {

        private readonly ILogger<ParkingController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ParkingfeeData _ParkingfeeData;
        public ParkingController(ILogger<ParkingController> logger, IConfiguration configuration, IOptions<ParkingfeeData> options)
        {
            _logger = logger;
            _configuration = configuration;
            _ParkingfeeData = options.Value;
        }

        private int GetAvaialbleParkingSpot()
        {
            string connecString = this._configuration.GetConnectionString("ParkingDatabase");
            int totalSpots = int.Parse(_ParkingfeeData.totalSpotsAvailable);
            int availableSlot;
            using (SqlConnection con = new SqlConnection(connecString))
            {
                using (SqlCommand cmd = new SqlCommand("[ParkingSpot].[dbo].[Getvailableslots]", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataReader sdr = cmd.ExecuteReader();
                    sdr.Read();
                    availableSlot = totalSpots - int.Parse(sdr["Availablespots"].ToString());
                }
                con.Close();

            }
            return availableSlot;
        }
        private bool CheckVehicleParkedOrLeft(string tagNumber)
        {
            bool isParked = false;
            string connecString = this._configuration.GetConnectionString("ParkingDatabase");

            using (SqlConnection con = new SqlConnection(connecString))
            {
                using (SqlCommand cmd = new SqlCommand("[ParkingSpot].[dbo].[CheckVehicleParkedOrLeft]", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tagnumber", tagNumber);
                    SqlDataReader sdr = cmd.ExecuteReader();
                    sdr.Read();
                    int retuVal = int.Parse(sdr["IsParked"].ToString());
                    if (retuVal > 0) { isParked = true; } else { isParked = false; }
                }
                con.Close();

            }
            return isParked;
        }
        private bool CheckValidCarParked(string tagNumber)
        {
            bool isParked = false;
            string connecString = this._configuration.GetConnectionString("ParkingDatabase");

            using (SqlConnection con = new SqlConnection(connecString))
            {
                using (SqlCommand cmd = new SqlCommand("[ParkingSpot].[dbo].[CheckValidCarParked]", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tagnumber", tagNumber);
                    SqlDataReader sdr = cmd.ExecuteReader();
                    sdr.Read();
                    int retuVal = int.Parse(sdr["IsCarExist"].ToString());
                    if (retuVal > 0) { isParked = true; } else { isParked = false; }
                }
                con.Close();

            }
            return isParked;
        }

        [HttpGet("GetTakenParkingSpot")]
        public async Task<IActionResult> GetTakenParkingSpot()
        {
            List<Parkingspot> parkingspots = new List<Parkingspot>();
            string connecString = this._configuration.GetConnectionString("ParkingDatabase");
            int totalSpots = int.Parse(_ParkingfeeData.totalSpotsAvailable);
            int availableSlot = 0;
            using (SqlConnection con = new SqlConnection(connecString))
            {
                using (SqlCommand cmd = new SqlCommand("[ParkingSpot].[dbo].[GetTakenParkingspots]", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            parkingspots.Add(new Parkingspot
                            {
                                TagNumber = sdr["TagNumber"].ToString(),
                                InTime = DateTime.Parse(sdr["InTime"].ToString()),
                                ElapsedTime = int.Parse(sdr["ElapsedTime"].ToString()),
                                Fee = int.Parse(sdr["Fee"].ToString()),
                            });
                        }
                    }
                    con.Close();
                }
                con.Close();
                availableSlot = totalSpots - parkingspots.Count;

            }

            GetSpots getSpots = new GetSpots(availableSlot, parkingspots.Count, parkingspots);
            return Ok(getSpots);
        }
        [HttpPost("InParkingSpot")]
        public IActionResult InParkingSpot(string tagNumber)
        {

            int i = 0;
            string connecString = this._configuration.GetConnectionString("ParkingDatabase");
            int availableSlot = GetAvaialbleParkingSpot();
            bool isParked = CheckVehicleParkedOrLeft(tagNumber);
            if (availableSlot > 0)
            {
                if (!isParked)
                {
                    using (SqlConnection con = new SqlConnection(connecString))
                    {
                        using (SqlCommand cmd = new SqlCommand("[ParkingSpot].[dbo].[InParkingspot]", con))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@tagnumber", tagNumber);
                            con.Open();
                            i = cmd.ExecuteNonQuery();
                        }
                        con.Close();

                    }
                }
                else
                {
                    return BadRequest("Car already Parked in");
                }
            }
            else
                return BadRequest("Slots are not available");
            if (i >= 1)
                return Ok(true);
            else
                return Ok(false);
        }
        [HttpPut("OutParkingSpot")]
        public IActionResult OutParkingSpot(string tagNumber)
        {
            int i = 0;
            string connecString = this._configuration.GetConnectionString("ParkingDatabase");
            bool isValid = CheckValidCarParked(tagNumber);
            if (isValid)
            {
                bool isParked = CheckVehicleParkedOrLeft(tagNumber);
                if (isParked)
                {
                    using (SqlConnection con = new SqlConnection(connecString))
                    {
                        int defaultFeeperHour = int.Parse(_ParkingfeeData.defaultFeeperHour);
                        SqlCommand cmd = new SqlCommand("[ParkingSpot].[dbo].[OutParkingspot]", con);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@tagnumber", tagNumber);
                        con.Open();
                        i = cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    return Ok(true);
                }
                else
                    return StatusCode(StatusCodes.Status200OK, "Car already left parking lot ");
            }
            return StatusCode(StatusCodes.Status200OK, "Car TagNumber not Valid ");
        }
        [HttpGet("GetModelStats")]
        public IActionResult GetModelStats()
        {
            ParkingModalStats parkingModalStats = new ParkingModalStats();
            int i = 0;
            string connecString = this._configuration.GetConnectionString("ParkingDatabase");
            SqlConnection con = new SqlConnection(connecString);

            SqlCommand cmd = new SqlCommand("[ParkingSpot].[dbo].[CalculatePakingstats]", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            con.Open();
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                sdr.Read();
                parkingModalStats.Availablespots = int.Parse(sdr["Availablespots"].ToString());
                parkingModalStats.todayRevenue = int.Parse(sdr["todayRevenue"].ToString());
                parkingModalStats.AvgCarsPerDay = int.Parse(sdr["AvgCarsPerDay"].ToString());
                parkingModalStats.AVGRevenuePerDay = int.Parse(sdr["AVGRevenuePerDay"].ToString());


            }
            con.Close();

            return Ok(parkingModalStats);

        }
    }
}
