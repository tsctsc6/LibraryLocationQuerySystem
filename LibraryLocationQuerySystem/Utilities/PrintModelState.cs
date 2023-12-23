using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryLocationQuerySystem.Utilities
{
    public class PrintModelState
    {
        static public void printErrorMessage(ModelStateDictionary ModelState)
        {
            foreach (var item in ModelState)
            {
                if (item.Value.Errors.Count != 0)
                {
                    Console.WriteLine(item.Key);
                    foreach (var item2 in item.Value.Errors)
                    {
                        Console.WriteLine(item2.ErrorMessage);
                    }
                }
            }
        }
    }
}
