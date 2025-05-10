namespace APBD_s31722_9_APi_2.DataLayer.Models;

public class WareHouseRequestDto
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    
}