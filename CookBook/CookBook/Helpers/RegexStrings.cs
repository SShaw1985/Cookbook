using System;
namespace CookBook.Helpers
{
    public class RegexStrings
    {
        public static string TelephoneRegex = @"(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?";
        public static string EmailRegex = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        public static string WebsiteRegex = @"([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?";
        public static string zipPattern = @"^\d{5}-\d{4}|\d{5}|[A-Z]\d[A-Z] \d[A-Z]\d$";
        public static string statePattern = @"\b(?:Ala(?:(?:bam|sk)a)|American Samoa|Arizona|Arkansas|(?:^(?!Baja )California)|Colorado|Connecticut|Delaware|District of Columbia|Florida|Georgia|Guam|Hawaii|Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|Massachusetts|Michigan|Minnesota|Miss(?:(?:issipp|our)i)|Montana|Nebraska|Nevada|New (?:Hampshire|Jersey|Mexico|York)|North (?:(?:Carolin|Dakot)a)|Ohio|Oklahoma|Oregon|Pennsylvania|Puerto Rico|Rhode Island|South (?:(?:Carolin|Dakot)a)|Tennessee|Texas|Utah|Vermont|Virgin(?:ia| Island(s?))|Washington|West Virginia|Wisconsin|Wyoming|A[KLRSZ]|C[AOT]|D[CE]|FL|G[AU]|HI|I[ADLN]|K[SY]|LA|M[ADEINOST]|N[CDEHJMVY]|O[HKR]|P[AR]|RI|S[CD]|T[NX]|UT|V[AIT]|W[AIVY])\b";
        public static string cityPattern = @"\b([A-Za-z]+\s?(?: [A-Za-z])*),? ([A-Za-z]{2}) \d{5}\b";
        public static string StreetAddressPattern = @"\d+(\s+\w+){1,}\s+(?:st(?:\.|reet)?|dr(?:\.|ive)?|pl(?:\.|ace)?|ave(?:\.|nue)?|rd|road|lane|drive|way|court|plaza|square|run|parkway|point|pike|square|driveway|trace|park|terrace|blvd)";
    }
}
