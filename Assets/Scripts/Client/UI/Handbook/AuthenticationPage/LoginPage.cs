using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LoginPage : AbstractBookPage
{
    public List<AbstractBookPage> pages;
    public AbstractBookPage choosingSubpage;
    
    private readonly List<char> _chars = new List<char>
    {
        '`', '~', '!', '@', '#', '$', '%', '^', '&', '*', 
        '(', ')', '_', '+', '=', '-', '[', ']', '{', '}', 
        ';', ':', ',', '<', '.', '>', '/', '?', '"', '\''
    };

    public void OnEnable()
    {
        SwitchDisplaySubpage(0);
    }

    public void SwitchDisplaySubpage(int index)
    {
        if (choosingSubpage != null)
            choosingSubpage.Close();

        choosingSubpage = pages[index];
        choosingSubpage.Open();
    }

    public char UsernameValidate(string text, int charIndex, char c)
    {
        return char.IsLetterOrDigit(c) || _chars.Contains(c) ? c : '\0';
    }

    public char PasswordValidate(string text, int charIndex, char c)
    {
        if (Regex.IsMatch(c.ToString(), "^[a-zA-Z0-9]+$") || _chars.Contains(c))
            return c;
        return '\0';
    }
}