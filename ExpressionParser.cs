using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Platform.Parser
{
    //表达式解析器
    //执行一个字符串表达式
    //支持变量，不支持位运算
    public class ExpressionParser : AbstractParser
    {
        //执行
        public virtual object Execute(params object[] parameters)
        {
            if(Function == null)
            {
                Expression exp = ParseAssignment();
                Console.WriteLine(exp);
                LambdaExpression lambda = Expression.Lambda(exp, Parameters.ToArray());
                Function = lambda.Compile();
            }
            if(Parameters.Count != 0 && parameters.Length == 0)
                parameters = new object[Parameters.Count];
            object result = Function.DynamicInvoke(parameters);
            Console.WriteLine(result.GetType().Name + ": " + result);
            return result;
        }
        

        //解析赋值 =
        protected Expression ParseAssignment()
        {
            Expression exp1 = ParseTernary();
            Expression exp2 = null;
            switch(_token)
            {
                case "=":
                    exp2 = ParseAssignment();
                    if(exp1.NodeType == ExpressionType.Parameter && exp1.Type == typeof(object) && exp1.Type != exp2.Type)
                        exp1 = Expression.Parameter(exp2.Type, (exp1 as ParameterExpression).Name);
                    exp1 = Expression.Assign(exp1, exp2);
                    break;
            }
            return exp1;
        }
        //解析三目 ?:
        protected Expression ParseTernary()
        {
            Expression exp1 = ParseLogicOr();
            Expression exp2 = null;
            Expression exp3 = null;
            switch(_token)
            {
                case "?":
                    exp2 = ParseLogicOr();
                    VerifyToken(":");
                    exp3 = ParseLogicOr();
                    exp1 = Expression.Condition(ConvertExpression(exp1, typeof(bool)), exp2, ConvertExpression(exp3, exp2.Type));
                    break;
                case "??":
                    exp2 = ParseLogicOr();
                    exp1 = Expression.Coalesce(exp1, exp2);
                    break;
            }
            return exp1;
        }
        //解析逻辑或 ||
        protected Expression ParseLogicOr()
        {
            Expression exp1 = ParseLogicAnd();
            Expression exp2 = null;
            while(true)
            {
                switch(_token)
                {
                    case "||":
                        exp2 = ParseLogicAnd();
                        exp1 = Expression.OrElse(ConvertExpression(exp1, typeof(bool)), ConvertExpression(exp2, typeof(bool)));
                        break;
                    default:
                        return exp1;
                }
            }
        }
        //解析逻辑与 &&
        protected Expression ParseLogicAnd()
        {
            Expression exp1 = ParseEquality();
            Expression exp2 = null;
            while(true)
            {
                switch(_token)
                {
                    case "&&":
                        exp2 = ParseEquality();
                        exp1 = Expression.AndAlso(ConvertExpression(exp1, typeof(bool)), ConvertExpression(exp2, typeof(bool)));
                        break;
                    default:
                        return exp1;
                }
            }
        }
        //解析位或 |
        //解析位异或 ^
        //解析位与 &
        //不支持

        //解析条件等 == !=
        protected Expression ParseEquality()
        {
            Expression exp1 = ParseComparison();
            Expression exp2 = null;
            switch(_token)
            {
                case "==":
                    exp2 = ParseComparison();
                    exp1 = Expression.Equal(exp1, ConvertExpression(exp2, exp1.Type));
                    break;
                case "!=":
                    exp2 = ParseComparison();
                    exp1 = Expression.NotEqual(exp1, ConvertExpression(exp2, exp1.Type));
                    break;
            }
            return exp1;
        }
        //解析关系 > >= < <=
        protected Expression ParseComparison()
        {
            Expression exp1 = ParseAddSubtract();
            Expression exp2 = null;
            switch(_token)
            {
                case ">":
                    exp2 = ParseAddSubtract();
                    exp1 = Expression.GreaterThan(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                    break;
                case ">=":
                    exp2 = ParseAddSubtract();
                    exp1 = Expression.GreaterThanOrEqual(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                    break;
                case "<":
                    exp2 = ParseAddSubtract();
                    exp1 = Expression.LessThan(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                    break;
                case "<=":
                    exp2 = ParseAddSubtract();
                    exp1 = Expression.LessThanOrEqual(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                    break;
            }
            return exp1;
        }
        //解析位移 >> <<
        //位运算 ^ & |
        //不支持

        //解析加减 + -
        protected Expression ParseAddSubtract()
        {
            Expression exp1 = ParseMultiplyDivide();
            Expression exp2 = null;
            while(true)
            {
                switch(_token)
                {
                    case "+":
                        exp2 = ParseMultiplyDivide();
                        if(exp1.Type == typeof(string) || exp2.Type == typeof(string))
                            exp1 = Expression.Call(
                                typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }), 
                                ConvertExpression(exp1, typeof(string)), 
                                ConvertExpression(exp2, typeof(string))
                            );
                        else
                            exp1 = Expression.Add(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                        break;
                    case "-":
                        exp2 = ParseMultiplyDivide();
                        exp1 = Expression.Subtract(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                        break;
                    default:
                        return exp1;
                }
            }
        }
        //解析乘除 * / %
        protected Expression ParseMultiplyDivide()
        {
            Expression exp1 = ParsePower();
            Expression exp2 = null;
            while(true)
            {
                switch(_token)
                {
                    case "*":
                        exp2 = ParsePower();
                        exp1 = Expression.Multiply(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                        break;
                    case "/":
                        exp2 = ParsePower();
                        exp1 = Expression.Divide(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                        break;
                    case "%":
                        exp2 = ParsePower();
                        exp1 = Expression.Modulo(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                        break;
                    default:
                        return exp1;
                }
            }
        }
        //幂运算 **
        protected Expression ParsePower()
        {
            Expression exp1 = ParseUnary();
            Expression exp2 = null;
            while(true)
            {
                switch(_token)
                {
                    case "**":
                        exp2 = ParseUnary();
                        exp1 = Expression.Power(ConvertExpression(exp1, typeof(double)), ConvertExpression(exp2, typeof(double)));
                        break;
                    default:
                        return exp1;
                }
            }
        }
        //解析单目 - !
        protected Expression ParseUnary()
        {
            Expression exp1 = null;
            NextToken();
            switch(_token)
            {
                case "-":
                    NextToken();
                    exp1 = ParseBasic();
                    exp1 = Expression.Negate(ConvertExpression(exp1, typeof(double)));
                    break;
                case "!":
                    NextToken();
                    exp1 = ParseBasic();
                    exp1 = Expression.Not(ConvertExpression(exp1, typeof(bool)));
                    break;
                default:
                    exp1 = ParseBasic();
                    break;
            }
            return exp1;
        }
        //解析基础运算及关键字 () [ ] . new, true, false
        protected Expression ParseBasic()
        {
            Expression exp1 = null;
            switch(_token)
            {
                case "(":
                    exp1 = ParseAssignment();
                    VerifyToken(")");
                    NextToken();
                    break;
                case "[":
                case ".":
                case "new":
                    ;
                    break;
                case "true":
                    exp1 = Expression.Constant(true);
                    break;
                case "false":
                    exp1 = Expression.Constant(false);
                    break;
                default:
                    switch(_tokenType)
                    {
                        case "Identify":
                            exp1 = ParseIdentify();
                            break;
                        case "Variable":
                            exp1 = ParseVariable();
                            break;
                        default:
                            exp1 = ParseConstant();
                            break;
                    }
                    break;
            }
            return exp1;
        }
        //解析标识符
        protected Expression ParseIdentify()
        {
            return ParseVariable();
        }
        //解析变量
        protected Expression ParseVariable()
        {
            Expression exp1 = Parameters.Find(item => item.Name == _token);
            if(exp1 == null)
            {
                exp1 = Expression.Variable(typeof(object), _token);
                Parameters.Add((ParameterExpression)exp1);
            }
            NextToken();
            return exp1;
        }
        //解析常量
        protected Expression ParseConstant()
        {
            Expression exp1 = null;
            switch(_tokenType)
            {
                case "Number":
                    exp1 = Expression.Constant(double.Parse(_token));
                    NextToken();
                    break;
                case "DateTime":
                    exp1 = Expression.Constant(DateTime.Parse(_token));
                    NextToken();
                    break;
                case "String":
                    exp1 = Expression.Constant(_token);
                    NextToken();
                    break;
                default:
                    throw new Exception(string.Format("type \"{0}\" with \"{1}\" at {2} is not allowed", _tokenType, _token, _position));
            }
            return exp1;
        }
        

        //转化表达式类型
        protected Expression ConvertExpression(Expression exp, Type outType)
        {
            if(exp.Type == outType)
            {
                return exp;
            }
            else
            {
                //if(outType == typeof(object))
                    return Expression.Convert(exp, outType);
                // else
                //     return Expression.Convert(
                //         Expression.Call(
                //             typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), 
                //             exp.Type == typeof(object) ? exp : Expression.Convert(exp, typeof(object)), 
                //             //exp,
                //             Expression.Constant(outType)
                //         ), outType
                //     );
            }
        }


        //语句
        public override string Statement 
        { 
            get
            {
                return _statement;
            }
            set
            {
                Function = null;
                Parameters.Clear();
                _statement = value;
            }
        }
        private string _statement;


        //委托函数
        public Delegate Function { get; protected set; }
        
        //参数
        public List<ParameterExpression> Parameters { get; } = new List<ParameterExpression>();

    }
}