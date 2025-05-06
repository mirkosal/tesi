using System;
using System.Text;

public class Person
{
    public int ID { get; set; }  // Aggiunto campo ID
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Diagnosis { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string TaxCode { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("ID: ").Append(ID).Append(" - "); // Aggiunta della stampa dell'ID
        sb.Append(FirstName);
        sb.Append(" ");
        sb.Append(LastName);
        sb.Append(" - ");
        sb.Append(Diagnosis);
        sb.Append(" - ");
        sb.Append(DateOfBirth.ToShortDateString());
        sb.Append(" - ");
        sb.Append(TaxCode);

        return sb.ToString();
    }
}
