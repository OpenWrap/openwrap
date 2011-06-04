using NUnit.Framework;
using Tests.Repositories.contexts;

namespace Tests.Repositories.factories.indexed_folder
{
    [TestFixture("c:\\middle-earth")]
    [TestFixture("c:\\middle-earth\\")]
    [TestFixture("c:\\middle-earth\\index.wraplist")]
    public class from_user_input_directory_doesnt_exist : indexed_folder_repository
    {
        public from_user_input_directory_doesnt_exist(string userInput)
        {
            when_detecting(userInput);
        }
    }
}