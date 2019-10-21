using System;

namespace Platform.Parser
{
    //抽象解析器
    //包含解析逐个语素的通用方法
    public abstract class AbstractParser
    {
        //获取字符
        protected char GetChar(int offset = 0)
        {
            int position = _position + offset;
            if(position < Statement.Length)
                return Statement[position];
            else
                return '\0';
        }
        //下一个
        protected void NextToken()
        {
            char c = GetChar();
            char c2;
            while(c == ' ' || c == '\t' || c == '\r' || c== '\n' || c == '\f')
            {
                _position++;
                c = GetChar();
            }
            _lastPostion = _position;
            switch(c)
            {
                case '\0':
                    _token = "\\0";
                    _tokenType = "Ender";
                    break;
                case '?':
                case ':':
                case '+':
                case '-':
                case '/':
                case '%':
                case '^':
                case '(':
                case ')':
                case '[':
                case ']':
                    _position++;
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = "Operator";
                    break;
                case '*':
                case '=':
                case '|':
                case '&':
                     _position++;
                    if(GetChar() == c)
                        _position++;
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = "Operator";
                    break;
                case '!':
                    _position++;
                    if(GetChar() == '=')
                        _position++;
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = "Operator";
                    break;
                case '>':
                case '<':
                    _position++;
                    if(GetChar() == '=' || GetChar() == c)
                        _position++;
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = "Operator";
                    break;
                case '@':
                    do {
                        _position++;
                        c2 = GetChar();
                    } while(char.IsLetter(c2) || char.IsNumber(c2) || c2 == '_');
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = "Variable";
                    break;
                case '\'':
                case '\"':
                    do {
                        _position++;
                    } while(GetChar() != c);
                    _position++;
                    _token = Statement.Substring(_lastPostion + 1, _position - _lastPostion - 2);
                    _tokenType = "String";
                    break;
                case '#':
                    do {
                        _position++;
                    } while(GetChar() != c);
                    _position++;
                    _token = Statement.Substring(_lastPostion + 1, _position - _lastPostion - 2);
                    _tokenType = "DateTime";
                    break;
                default:
                    if(char.IsNumber(c))
                    {
                        do {
                            _position++;
                            c2 = GetChar();
                        } while(char.IsNumber(c2) || c2 == '.');
                        _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                        _tokenType = "Number";
                    }
                    else if(char.IsLetter(c))
                    {
                        do {
                            _position++;
                            c2 = GetChar();
                        } while(char.IsLetter(c2) || char.IsNumber(c2) || c2 == '_');
                        _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                        _tokenType = "Identify";
                    }
                    else
                    {
                        _position++;
                        _token = c.ToString();
                        _tokenType = "Unknown";
                    }
                    break;
            }
            Console.WriteLine(_position + "\t" + _tokenType + "  \t" + _token);
        }
        //校验语素
        protected void VerifyToken(string token)
        {
            if(token != _token)
                throw new Exception(string.Format("statement \"{0}\" at {1} must be \"{2}\"", _token, _position, token));
        }


        //语句
        public abstract string Statement { get; set; }

        
        //开始位置
        protected int _lastPostion;
        protected int _position;
        //语素片段
        protected string _token;
        //语素类型
        protected string _tokenType;
    }
}