namespace Sisfarma.Client.Unycop
{
    public class UnycopResponse
    {
        public string CodResp { get; set; }

        public string DescripResp { get; set; }

        public string Clave { get; set; }

        public FicheroResultInfo[] Ficheros { get; set; }
    }
}