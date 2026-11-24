namespace Server
{
    /// <summary>
    /// Just a stub standing in for a real authentication system.
    /// </summary>
    public class MockAuthenticator
    {
        private const string validUsername = "Thirteen";
        private const string validPassword = "Games";

        // Singleton
        public static MockAuthenticator Instance { get; private set; } = new MockAuthenticator();
        
        private MockAuthenticator() { }

        public bool Authenticate(string username, string password)
        {
            bool isAuthenticated = username == validUsername && password == validPassword;
            return isAuthenticated;
        }
    }
}