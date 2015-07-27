using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using WanTai.DataModel;

namespace WanTai.Controller
{
    public class ReagentAndSuppliesController
    {
        public void AddReagentAndSupplies(ReagentAndSupply reagentAndSupply)
        {
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                entities.ReagentAndSupplies.AddObject(reagentAndSupply);
                entities.SaveChanges();
            }
        }

        public List<ReagentAndSuppliesConfiguration> GetPCRHeatLiquidPlate()
        {
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                return entities.ReagentAndSuppliesConfigurations.Where(ReagentAndSupplies => ReagentAndSupplies.ItemType==205).ToList();
            }
        }
        public List<ReagentAndSupply> GetAll(Guid experimentID)
        {
            List<ReagentAndSupply> list = new List<ReagentAndSupply>();
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                list = entities.ReagentAndSupplies.Where(P => P.ExperimentID == experimentID).ToList();
            }
            return list;
        }

        public List<ReagentAndSupply> GetAllByType(int typeId)
        {
            List<ReagentAndSupply> list = new List<ReagentAndSupply>();
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                list = entities.ReagentAndSupplies.Where(P => P.ExperimentID == SessionInfo.ExperimentID && P.ItemType == typeId).ToList();
            }
            return list;
        }

        //only can be used when typeId>=100
        public List<ReagentAndSupply> GetAllByTypeAndRotationId(int typeId, Guid rotationId)
        {
            List<ReagentAndSupply> list = new List<ReagentAndSupply>();
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "select distinct ReagentAndSupplies.* from ReagentAndSupplies inner join ReagentsAndSuppliesConsumption"
                    + " on ReagentAndSupplies.ItemID =ReagentsAndSuppliesConsumption.ReagentAndSupplieID"
                    + " where ItemType=@ItemType and ReagentsAndSuppliesConsumption.RotationID=@RotationID"
                    + " and ReagentAndSupplies.ExperimentID=@ExperimentID";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemType", typeId);
                        cmd.Parameters.AddWithValue("@RotationID", rotationId);
                        cmd.Parameters.AddWithValue("@ExperimentID", SessionInfo.ExperimentID);

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read())
                            {
                                list.Add(new ReagentAndSupply() { ItemID = (Guid)reader["ItemID"], ItemType = (short)reader["ItemType"] });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetNextLogs", SessionInfo.ExperimentID);
                throw;
            }

            return list;
        }

        public Guid GetReagentID(Guid experimentID, string barcodePrefix)
        {
            Guid reagentID = Guid.Empty;
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                ReagentAndSupply reagent = entities.ReagentAndSupplies.FirstOrDefault(
                    P => (P.ExperimentID == experimentID && P.BarCode.StartsWith(barcodePrefix)));
                if (reagent != null)
                    reagentID = reagent.ItemID;
            }
            return reagentID;
        }

        //only can be used when typeId>=100
        public List<ReagentAndSupply> GetReagentID(Guid rotationID)
        {
            List<ReagentAndSupply> supplies = new List<ReagentAndSupply>();
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string strSql = @"select distinct ReagentAndSupplies.* from ReagentAndSupplies
                    left join ReagentsAndSuppliesConsumption
                    on ReagentAndSupplies.ItemID =ReagentsAndSuppliesConsumption.ReagentAndSupplieID
                    where  ReagentsAndSuppliesConsumption.RotationID=@RotationID";
                using(SqlConnection conn=new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand comm = new SqlCommand(strSql, conn);
                    comm.Parameters.AddWithValue("@RotationID", rotationID);
                    

                    SqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        supplies.Add(new ReagentAndSupply() { ItemID = (Guid)reader["ItemID"], ItemType = (short)reader["ItemType"], ConfigurationItemID = (Guid)reader["ConfigurationItemID"] });
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
            return supplies;
        }
    }
}
