
CREATE PROCEDURE AddProductToWarehouse @IdProduct INT, @IdWarehouse INT, @Amount INT,  
@CreatedAt DATETIME
AS  
BEGIN  
   
 DECLARE @IdProductFromDb INT, @IdOrder INT, @Price DECIMAL(5,2);   
 SELECT TOP 1 @IdOrder = o.IdOrder  
 FROM [Order] o   
 WHERE o.IdProduct=@IdProduct 
   AND o.Amount=@Amount 
   AND o.CreatedAt<@CreatedAt
   AND NOT EXISTS(
       SELECT 1 
       FROM Product_Warehouse pw
       WHERE pw.IdOrder = o.IdOrder
 )  
   
  
 SELECT @IdProductFromDb=Product.IdProduct, @Price=Product.Price FROM Product WHERE IdProduct=@IdProduct  
   
 IF @IdProductFromDb IS NULL  
 BEGIN  
  RAISERROR('Invalid parameter: Provided IdProduct does not exist', 16, 0);  
  RETURN;  
 END;  
  
 IF @IdOrder IS NULL  
 BEGIN  
  RAISERROR('Invalid parameter: There is no order to fullfill', 16, 0);  
  RETURN;  
 END;  
   
 IF NOT EXISTS(SELECT 1 FROM Warehouse WHERE IdWarehouse=@IdWarehouse)  
 BEGIN  
  RAISERROR('Invalid parameter: Provided IdWarehouse does not exist', 16, 0);  
  RETURN;  
 END;  
  
 SET XACT_ABORT ON;  
 BEGIN TRAN;  
   
 UPDATE [Order] SET  
 FulfilledAt=@CreatedAt  
 WHERE IdOrder=@IdOrder;  
  
 INSERT INTO Product_Warehouse(IdWarehouse,   
 IdProduct, IdOrder, Amount, Price, CreatedAt)  
 VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Amount*@Price, @CreatedAt);  
   
 SELECT SCOPE_IDENTITY() AS NewId;
   
 COMMIT;  
END