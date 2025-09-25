namespace MotoModel.Entities;

public class Moto

{
public required int id {get; set;}
public required string tipoMoto {get; set;}
public required string placa {get; set;}
public required string numChassi {get; set;}
public int? TagRfidId {get; set;}
}

