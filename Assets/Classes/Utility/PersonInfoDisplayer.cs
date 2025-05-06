using UnityEngine;
using TMPro;

public class PersonInfoDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lblFullName; // Label per Nome e Cognome
    [SerializeField] private TextMeshProUGUI lblDiagnosis;
    [SerializeField] private TextMeshProUGUI lblDateOfBirth;
    [SerializeField] private TextMeshProUGUI lblTaxCode;

    public void DisplayPersonInfo(Person person)
    {
        lblFullName.text = $"{person.FirstName} {person.LastName}";
        lblDiagnosis.text = person.Diagnosis;
        lblDateOfBirth.text = person.DateOfBirth.ToString("dd/MM/yyyy");
        lblTaxCode.text = person.TaxCode;
    }
}
