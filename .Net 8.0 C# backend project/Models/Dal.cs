using Azure;
using Microsoft.Data.SqlClient;
using TheGadgetHub.Util;

namespace TheGadgetHub.Models
{
    public class Dal
    {
        //Add gadgets to the database
        public Response AddGadget(Gadget gadget, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("Insert into Gadget(GName,GDescription,GPrice,GImagePath)VALUEs" +
                    "(@GName,@GDescription,@GPrice,@GImagePath)", dbc.GetConn());
                cmd.Parameters.AddWithValue("@GName", gadget.GName);
                cmd.Parameters.AddWithValue("GDescription", gadget.GDescription);
                cmd.Parameters.AddWithValue("GPrice", gadget.GPrice);
                cmd.Parameters.AddWithValue("GImagePath", gadget.GImagePath);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Gadget Added Successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Failed to add gadget";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }
            return response;
        }

        //Get gadget details from the database
        public List<Gadget> GetAllGadget(SqlConnection connection)
        {
            List<Gadget> gadgetList = new List<Gadget>();
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Gadget", connection);
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Gadget gadget = new Gadget
                    {
                        GId = Convert.ToInt32(reader["GId"]),
                        GName = reader["GName"].ToString(),
                        GDescription = reader["GDescription"] != DBNull.Value ? reader["GDescription"].ToString() : null, 
                        GPrice = Convert.ToInt32(reader["GPrice"]),
                        GImagePath = reader["GImagePath"] != DBNull.Value ? reader["GImagePath"].ToString() : null,
                        GStatus = Convert.ToInt32(reader["GStatus"])
                    };
                    gadgetList.Add(gadget);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return gadgetList;
        }

        //Add cutomer orders to the database
        public Response PlaceOrder(OrderRequest order, SqlConnection connection)
        {
            Response response = new Response();
            if (connection.State != System.Data.ConnectionState.Open) connection.Open();

            // Start Transaction: If one part fails, everything is rolled back
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // 1. Insert into Orders Table
                string orderSql = @"INSERT INTO Orders (CustomerName, CustomerPhone, Address, TotalAmount, OrderDate) 
                            VALUES (@CustomerName, @CustomerPhone, @Address, @TotalAmount, GETDATE());
                            SELECT SCOPE_IDENTITY();"; // Gets the OrderId just created

                SqlCommand cmd = new SqlCommand(orderSql, connection, transaction);
                cmd.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                cmd.Parameters.AddWithValue("@CustomerPhone", order.CustomerPhone);
                cmd.Parameters.AddWithValue("@Address", order.Address);
                cmd.Parameters.AddWithValue("@TotalAmount", order.Total);

                int newOrderId = Convert.ToInt32(cmd.ExecuteScalar());

                // 2. Insert into OrderItems Table
                string itemSql = @"INSERT INTO OrderItems (OrderId, GId, Quantity, GPrice) 
                           VALUES (@OrderId, @GId, @Quantity, @GPrice)";

                foreach (var item in order.Items)
                {
                    SqlCommand itemCmd = new SqlCommand(itemSql, connection, transaction);
                    itemCmd.Parameters.AddWithValue("@OrderId", newOrderId);
                    itemCmd.Parameters.AddWithValue("@GId", item.GId);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@GPrice", item.GPrice);
                    itemCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                response.StatusCode = 200;
                response.StatusMessage = "Order Placed Successfully";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                response.StatusCode = 500;
                response.StatusMessage = "Error: " + ex.Message;
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        //Get all order details
        public List<OrderRequest> GetAllOrders(SqlConnection connection)
        {
            List<OrderRequest> orders = new List<OrderRequest>();
            try
            {
                connection.Open();

                // 1. Get all main orders
                string sqlOrders = "SELECT * FROM Orders ORDER BY OrderDate DESC";
                SqlCommand cmd = new SqlCommand(sqlOrders, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new OrderRequest
                    {
                        OrderId = Convert.ToInt32(reader["OrderId"]),
                        CustomerName = reader["CustomerName"].ToString(),
                        CustomerPhone = reader["CustomerPhone"].ToString(),
                        Address = reader["Address"].ToString(),
                        Total = Convert.ToDecimal(reader["TotalAmount"]),
                        OrderDate = Convert.ToDateTime(reader["OrderDate"])
                    });
                }
                reader.Close();

                // 2. Get items for each order
                foreach (var order in orders)
                {
                    string sqlItems = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
                    SqlCommand itemCmd = new SqlCommand(sqlItems, connection);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    SqlDataReader itemReader = itemCmd.ExecuteReader();

                    while (itemReader.Read())
                    {
                        order.Items.Add(new CartItem
                        {
                            GId = Convert.ToInt32(itemReader["GId"]),
                            Quantity = Convert.ToInt32(itemReader["Quantity"]),
                            GPrice = Convert.ToDecimal(itemReader["GPrice"])
                        });
                    }
                    itemReader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return orders;
        }

        //Get order product details to the quatation request
        public List<OrderRequest> QuatationRequest(SqlConnection connection)
        {
            List<OrderRequest> orders = new List<OrderRequest>();
            try
            {
                connection.Open();

                // 1. Get all main orders
                string sqlOrders = "SELECT * FROM Orders ORDER BY OrderDate DESC";
                SqlCommand cmd = new SqlCommand(sqlOrders, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new OrderRequest
                    {
                        OrderId = Convert.ToInt32(reader["OrderId"]),
                        CustomerName = reader["CustomerName"].ToString(),
                        OrderDate = Convert.ToDateTime(reader["OrderDate"])
                    });
                }
                reader.Close();

                // 2. Get items for each order
                foreach (var order in orders)
                {
                    string sqlItems = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
                    SqlCommand itemCmd = new SqlCommand(sqlItems, connection);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    SqlDataReader itemReader = itemCmd.ExecuteReader();

                    while (itemReader.Read())
                    {
                        order.Items.Add(new CartItem
                        {
                            GId = Convert.ToInt32(itemReader["GId"]),
                            Quantity = Convert.ToInt32(itemReader["Quantity"])
                        });
                    }
                    itemReader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return orders;
        }

        //Add formulate quatations to the tech world table
        public Response AddTechWorldFormulateQuatation(FormulateQuatationTechWorld fqtw, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("Insert into FormulateQuatationsTechWorld(OrderId,GId,PricePerUnit,NumberOfUnit,Availability,DeliveryTime)VALUEs" +
                    "(@OrderId,@GId,@PricePerUnit,@NumberOfUnit,@Availability,@DeliveryTime)", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderId", fqtw.OrderId);
                cmd.Parameters.AddWithValue("@GId", fqtw.GId);
                cmd.Parameters.AddWithValue("@PricePerUnit", fqtw.PricePerUnit);
                cmd.Parameters.AddWithValue("@NumberOfUnit", fqtw.NumberOfUnit);
                cmd.Parameters.AddWithValue("@Availability", fqtw.Availability);
                cmd.Parameters.AddWithValue("@DeliveryTime", fqtw.DeliveryTime);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Quatation Formulation Added Successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Failed to add Formulation";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }
            return response;
        }

        //Get formulation quatations of tech world
        public List<OrderRequestDetails> GetTechWorldFormulateQuatation(SqlConnection connection)
        {
            List<OrderRequestDetails> orders = new List<OrderRequestDetails>();
            try
            {
                connection.Open();

                // 1. Get all main orders
                string sqlOrders = "SELECT * FROM Orders ORDER BY OrderDate DESC";
                SqlCommand cmd = new SqlCommand(sqlOrders, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new OrderRequestDetails
                    {
                        OrderId = Convert.ToInt32(reader["OrderId"]),
                        CustomerName = reader["CustomerName"].ToString(),
                        CustomerPhone = reader["CustomerPhone"].ToString(),
                        Address = reader["Address"].ToString(),
                        Total = Convert.ToDecimal(reader["TotalAmount"]),
                        OrderDate = Convert.ToDateTime(reader["OrderDate"])
                    });
                }
                reader.Close();

                // 2. Get quatations for each order
                foreach (var order in orders)
                {
                    string sqlItems = "SELECT * FROM FormulateQuatationsTechWorld WHERE OrderId = @OrderId";
                    SqlCommand itemCmd = new SqlCommand(sqlItems, connection);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    SqlDataReader itemReader = itemCmd.ExecuteReader();

                    while (itemReader.Read())
                    {
                        order.Items.Add(new FormulateQuatationTechWorld
                        {
                            FormulateId = Convert.ToInt32(itemReader["FormulateId"]),
                            OrderId = Convert.ToInt32(itemReader["OrderId"]),
                            GId = Convert.ToInt32(itemReader["GId"]),
                            PricePerUnit = Convert.ToDecimal(itemReader["PricePerUnit"]),
                            NumberOfUnit = Convert.ToInt32(itemReader["NumberOfUnit"]),
                            Availability = itemReader["Availability"].ToString(),
                            DeliveryTime = Convert.ToDateTime(itemReader["DeliveryTime"])
                        });
                    }
                    itemReader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return orders;
        }

        //Add formulate quatations to the ElectroCom table
        public Response AddElectroComFormulateQuatation(FormulateQuatationElectroCom fqec, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("Insert into FormulateQuatationsElectroCom(OrderId,GId,PricePerUnit,NumberOfUnit,Availability,DeliveryTime)VALUEs" +
                    "(@OrderId,@GId,@PricePerUnit,@NumberOfUnit,@Availability,@DeliveryTime)", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderId", fqec.OrderId);
                cmd.Parameters.AddWithValue("@GId", fqec.GId);
                cmd.Parameters.AddWithValue("@PricePerUnit", fqec.PricePerUnit);
                cmd.Parameters.AddWithValue("@NumberOfUnit", fqec.NumberOfUnit);
                cmd.Parameters.AddWithValue("@Availability", fqec.Availability);
                cmd.Parameters.AddWithValue("@DeliveryTime", fqec.DeliveryTime);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Quatation Formulation Added Successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Failed to add Formulation";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }
            return response;
        }

        //Get formulation quatations of ElectroCom
        public List<OrderRequestDetailsElectroCom> GetElectroComFormulateQuatation(SqlConnection connection)
        {
            List<OrderRequestDetailsElectroCom> orders = new List<OrderRequestDetailsElectroCom>();
            try
            {
                connection.Open();

                // 1. Get all main orders
                string sqlOrders = "SELECT * FROM Orders ORDER BY OrderDate DESC";
                SqlCommand cmd = new SqlCommand(sqlOrders, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new OrderRequestDetailsElectroCom
                    {
                        OrderId = Convert.ToInt32(reader["OrderId"]),
                        CustomerName = reader["CustomerName"].ToString(),
                        CustomerPhone = reader["CustomerPhone"].ToString(),
                        Address = reader["Address"].ToString(),
                        Total = Convert.ToDecimal(reader["TotalAmount"]),
                        OrderDate = Convert.ToDateTime(reader["OrderDate"])
                    });
                }
                reader.Close();

                // 2. Get quatations for each order
                foreach (var order in orders)
                {
                    string sqlItems = "SELECT * FROM FormulateQuatationsElectroCom WHERE OrderId = @OrderId";
                    SqlCommand itemCmd = new SqlCommand(sqlItems, connection);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    SqlDataReader itemReader = itemCmd.ExecuteReader();

                    while (itemReader.Read())
                    {
                        order.Items.Add(new FormulateQuatationElectroCom
                        {
                            FormulateId = Convert.ToInt32(itemReader["FormulateId"]),
                            OrderId = Convert.ToInt32(itemReader["OrderId"]),
                            GId = Convert.ToInt32(itemReader["GId"]),
                            PricePerUnit = Convert.ToDecimal(itemReader["PricePerUnit"]),
                            NumberOfUnit = Convert.ToInt32(itemReader["NumberOfUnit"]),
                            Availability = itemReader["Availability"].ToString(),
                            DeliveryTime = Convert.ToDateTime(itemReader["DeliveryTime"])
                        });
                    }
                    itemReader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return orders;
        }

        //Add formulate quatations to the gadget central table
        public Response AddGadgetCentralFormulateQuatation(FormulateQuatationGadgetCentral fqgc, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("Insert into FormulateQuatationsGadgetCentral(OrderId,GId,PricePerUnit,NumberOfUnit,Availability,DeliveryTime)VALUEs" +
                    "(@OrderId,@GId,@PricePerUnit,@NumberOfUnit,@Availability,@DeliveryTime)", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderId", fqgc.OrderId);
                cmd.Parameters.AddWithValue("@GId", fqgc.GId);
                cmd.Parameters.AddWithValue("@PricePerUnit", fqgc.PricePerUnit);
                cmd.Parameters.AddWithValue("@NumberOfUnit", fqgc.NumberOfUnit);
                cmd.Parameters.AddWithValue("@Availability", fqgc.Availability);
                cmd.Parameters.AddWithValue("@DeliveryTime", fqgc.DeliveryTime);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Quatation Formulation Added Successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Failed to add Formulation";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }
            return response;
        }

        //Get formulation quatations of Gadget Central
        public List<OrderRequestDetailsGadgetCentral> GetGadgetCentralFormulateQuatation(SqlConnection connection)
        {
            List<OrderRequestDetailsGadgetCentral> orders = new List<OrderRequestDetailsGadgetCentral>();
            try
            {
                connection.Open();

                // 1. Get all main orders
                string sqlOrders = "SELECT * FROM Orders ORDER BY OrderDate DESC";
                SqlCommand cmd = new SqlCommand(sqlOrders, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new OrderRequestDetailsGadgetCentral
                    {
                        OrderId = Convert.ToInt32(reader["OrderId"]),
                        CustomerName = reader["CustomerName"].ToString(),
                        CustomerPhone = reader["CustomerPhone"].ToString(),
                        Address = reader["Address"].ToString(),
                        Total = Convert.ToDecimal(reader["TotalAmount"]),
                        OrderDate = Convert.ToDateTime(reader["OrderDate"])
                    });
                }
                reader.Close();

                // 2. Get quatations for each order
                foreach (var order in orders)
                {
                    string sqlItems = "SELECT * FROM FormulateQuatationsGadgetCentral WHERE OrderId = @OrderId";
                    SqlCommand itemCmd = new SqlCommand(sqlItems, connection);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    SqlDataReader itemReader = itemCmd.ExecuteReader();

                    while (itemReader.Read())
                    {
                        order.Items.Add(new FormulateQuatationGadgetCentral
                        {
                            FormulateId = Convert.ToInt32(itemReader["FormulateId"]),
                            OrderId = Convert.ToInt32(itemReader["OrderId"]),
                            GId = Convert.ToInt32(itemReader["GId"]),
                            PricePerUnit = Convert.ToDecimal(itemReader["PricePerUnit"]),
                            NumberOfUnit = Convert.ToInt32(itemReader["NumberOfUnit"]),
                            Availability = itemReader["Availability"].ToString(),
                            DeliveryTime = Convert.ToDateTime(itemReader["DeliveryTime"])
                        });
                    }
                    itemReader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return orders;
        }

        //Add orders to the TechWorld
        public Response AddTechWorldOrders(TechWorldOrders two, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("Insert into TechWorldOrders(OrderId)VALUEs" +
                    "(@OrderId)", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderID", two.OrderId);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Order Added Successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Failed to add order";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }
            return response;
        }

        //Confirm TechWorld orders
        public Response ConfirmTechWorldOrders(TechWorldOrders two, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("UPDATE TechWorldOrders SET Confirmation=@Confirmation WHERE OrderId=@OrderId", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderId", two.OrderId);
                cmd.Parameters.AddWithValue("@Confirmation", two.Confirmation);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Order Confirmed Successfully";
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "Order Not Found";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = "Error: " + ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }

            return response;
        }

        //Add orders to the ElectroCom
        public Response AddElectroComOrders(ElectroComOrders eco, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("Insert into ElectroComOrders(OrderId)VALUEs" +
                    "(@OrderId)", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderID", eco.OrderId);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Order Added Successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Failed to add order";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }
            return response;
        }

        //Confirm ElectroCom orders
        public Response ConfirmElectroComOrders(ElectroComOrders eco, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("UPDATE ElectroComOrders SET Confirmation=@Confirmation WHERE OrderId=@OrderId", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderId", eco.OrderId);
                cmd.Parameters.AddWithValue("@Confirmation", eco.Confirmation);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Order Confirmed Successfully";
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "Order Not Found";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = "Error: " + ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }

            return response;
        }

        //Add orders to the Gadget Central
        public Response AddGadgetCentralOrders(GadgetCentralOrders gco, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("Insert into GadgetCentralOrders(OrderId)VALUEs" +
                    "(@OrderId)", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderID", gco.OrderId);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Order Added Successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Failed to add order";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }
            return response;
        }

        //Confirm Gadget Central orders
        public Response ConfirmGadgetCentralOrders(GadgetCentralOrders gco, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("UPDATE GadgetCentralOrders SET Confirmation=@Confirmation WHERE OrderId=@OrderId", dbc.GetConn());
                cmd.Parameters.AddWithValue("@OrderId", gco.OrderId);
                cmd.Parameters.AddWithValue("@Confirmation", gco.Confirmation);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Order Confirmed Successfully";
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "Order Not Found";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = "Error: " + ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }

            return response;
        }

        //Add notification
        public Response AddNotification(Notification notification, SqlConnection connection)
        {
            Response response = new Response();
            DBConnection dbc = new DBConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("Insert into Notification(Topic,Description)VALUEs" +
                    "(@Topic,@Description)", dbc.GetConn());
                cmd.Parameters.AddWithValue("@Topic", notification.Topic);
                cmd.Parameters.AddWithValue("@Description", notification.Description);

                dbc.ConOpen();
                int rowAffected = cmd.ExecuteNonQuery();
                dbc.ConClose();

                if (rowAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Notification Added Successfully";
                }
                else
                {
                    response.StatusCode = 400;
                    response.StatusMessage = "Failed to Add Notification";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = ex.Message;
            }
            finally
            {
                dbc.ConClose();
            }
            return response;
        }

        //Get all notifications
        public List<Notification> GetAllNotifications(SqlConnection connection)
        {
            List<Notification> notificationList = new List<Notification>();
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Notification", connection);
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Notification notification = new Notification
                    {
                        NotificationId = Convert.ToInt32(reader["NotificationId"]),
                        Topic = reader["Topic"].ToString(),
                        Description = reader["Description"].ToString()
                       
                    };
                    notificationList.Add(notification);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return notificationList;
        }

        //Get ElectroCom order details
        public List<ElectroComOrders> GetElectroOrderDetails(SqlConnection connection)
        {
            List<ElectroComOrders> orders = new List<ElectroComOrders>();
            try
            {
                connection.Open();

                string sqlOrders = "SELECT * FROM ElectroComOrders";
                SqlCommand cmd = new SqlCommand(sqlOrders, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new ElectroComOrders
                    {
                        DistributorOrderId = Convert.ToInt32(reader["DistributorOrderId"]),
                        OrderId = Convert.ToInt32(reader["OrderId"])
                    });
                }
                reader.Close();

                foreach (var order in orders)
                {
                    string sqlItems = "SELECT * FROM Orders WHERE OrderId = @OrderId";
                    SqlCommand itemCmd = new SqlCommand(sqlItems, connection);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    SqlDataReader itemReader = itemCmd.ExecuteReader();

                    while (itemReader.Read())
                    {
                        order.Items.Add(new ODRElectroCom
                        {
                            CustomerName = itemReader["CustomerName"].ToString(),                    
                            CustomerPhone = itemReader["CustomerPhone"].ToString(),
                            Address = itemReader["Address"].ToString(),
                            Total = Convert.ToDecimal(itemReader["TotalAmount"]),
                            OrderDate = Convert.ToDateTime(itemReader["OrderDate"])
                        });
                    }
                    itemReader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return orders;
        }

        //Get Gadget Central order details
        public List<GadgetCentralOrders> GetGadgetCentralDetails(SqlConnection connection)
        {
            List<GadgetCentralOrders> orders = new List<GadgetCentralOrders>();
            try
            {
                connection.Open();

                string sqlOrders = "SELECT * FROM GadgetCentralOrders";
                SqlCommand cmd = new SqlCommand(sqlOrders, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new GadgetCentralOrders
                    {
                        DistributorOrderId = Convert.ToInt32(reader["DistributorOrderId"]),
                        OrderId = Convert.ToInt32(reader["OrderId"])
                    });
                }
                reader.Close();

                foreach (var order in orders)
                {
                    string sqlItems = "SELECT * FROM Orders WHERE OrderId = @OrderId";
                    SqlCommand itemCmd = new SqlCommand(sqlItems, connection);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    SqlDataReader itemReader = itemCmd.ExecuteReader();

                    while (itemReader.Read())
                    {
                        order.Items.Add(new ODREGadgetCentral
                        {
                            CustomerName = itemReader["CustomerName"].ToString(),
                            CustomerPhone = itemReader["CustomerPhone"].ToString(),
                            Address = itemReader["Address"].ToString(),
                            Total = Convert.ToDecimal(itemReader["TotalAmount"]),
                            OrderDate = Convert.ToDateTime(itemReader["OrderDate"])
                        });
                    }
                    itemReader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return orders;
        }

        //Get TechWorld order details
        public List<TechWorldOrders> GetTechWorldDetails(SqlConnection connection)
        {
            List<TechWorldOrders> orders = new List<TechWorldOrders>();
            try
            {
                connection.Open();

                string sqlOrders = "SELECT * FROM TechWorldOrders";
                SqlCommand cmd = new SqlCommand(sqlOrders, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new TechWorldOrders
                    {
                        DistributorOrderId = Convert.ToInt32(reader["DistributorOrderId"]),
                        OrderId = Convert.ToInt32(reader["OrderId"])
                    });
                }
                reader.Close();

                foreach (var order in orders)
                {
                    string sqlItems = "SELECT * FROM Orders WHERE OrderId = @OrderId";
                    SqlCommand itemCmd = new SqlCommand(sqlItems, connection);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    SqlDataReader itemReader = itemCmd.ExecuteReader();

                    while (itemReader.Read())
                    {
                        order.Items.Add(new ODRETechWorld
                        {
                            CustomerName = itemReader["CustomerName"].ToString(),
                            CustomerPhone = itemReader["CustomerPhone"].ToString(),
                            Address = itemReader["Address"].ToString(),
                            Total = Convert.ToDecimal(itemReader["TotalAmount"]),
                            OrderDate = Convert.ToDateTime(itemReader["OrderDate"])
                        });
                    }
                    itemReader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return orders;
        }
    }
}
