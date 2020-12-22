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
                //结束符
                case '\0':
                    _token = "";
                    _tokenType = TokenType.Ender;
                    break;
                //单符号
                case '.':
                case ':':
                case '(':
                case ')':
                case '[':
                case ']':
                    _position++;
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = TokenType.Operator;
                    break;
                //单符号或带等号
                case '!':
                case '%':
                    _position++;
                    if(GetChar() == '=')
                        _position++;
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = TokenType.Operator;
                    break;
                //单符号或重叠符号
                case '?':
                case '=':
                     _position++;
                    if(GetChar() == c)
                        _position++;
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = TokenType.Operator;
                    break;
                //单符号或重叠或带等号
                case '+':
                case '-':
                case '*':
                case '/':
                case '>':
                case '<':
                case '|':
                case '&':
                    _position++;
                    if(GetChar() == '=' || GetChar() == c)
                        _position++;
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = TokenType.Operator;
                    break;
                //变量
                case '@':
                    do {
                        _position++;
                        c2 = GetChar();
                    } while(char.IsLetter(c2) || char.IsNumber(c2) || c2 == '_');
                    _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                    _tokenType = TokenType.Variable;
                    break;
                //字符串
                case '\'':
                case '\"':
                    do {
                        _position++;
                    } while(GetChar() != c);
                    _position++;
                    _token = Statement.Substring(_lastPostion + 1, _position - _lastPostion - 2);
                    _tokenType = TokenType.String;
                    break;
                //日期
                case '#':
                    do {
                        _position++;
                    } while(GetChar() != c);
                    _position++;
                    _token = Statement.Substring(_lastPostion + 1, _position - _lastPostion - 2);
                    _tokenType = TokenType.DateTime;
                    break;
                default:
                    //数字
                    if(char.IsNumber(c))
                    {
                        do {
                            _position++;
                            c2 = GetChar();
                        } while(char.IsNumber(c2) || c2 == '.');
                        _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                        _tokenType = TokenType.Number;
                    }
                    //标识符
                    else if(char.IsLetter(c) || c == '_')
                    {
                        do {
                            _position++;
                            c2 = GetChar();
                        } while(char.IsLetter(c2) || char.IsNumber(c2) || c2 == '_');
                        _token = Statement.Substring(_lastPostion, _position - _lastPostion);
                        _tokenType = TokenType.Identify;
                    }
                    else
                    {
                        _position++;
                        _token = c.ToString();
                        _tokenType = TokenType.Unknown;
                    }
                    break;
            }
            Console.WriteLine(_position + "\t" + _tokenType + "  \t" + _token);
        }
        //校验语素
        protected void VerifyToken(string token)
        {
            if(token != _token)
                throw new Exception($"statement \"{_token}\" at {_position} must be \"{token}\"");
        }


        //语句
        public abstract string Statement { get; set; }

        
        //开始位置
        protected int _lastPostion;
        protected int _position;
        //语素片段
        protected string _token;
        //语素类型
        protected TokenType _tokenType;
    }

    //语素类型
    public enum TokenType
    {
        Unknown,
        Operator,
        Identify,
        Variable,
        Number,
        DateTime,
        String,
        Ender,
    }
}