using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OpenWrap.Windows.Framework
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T, object>> propertyLambda)
        {
            if (PropertyChanged != null)
            {
                string propertyName = ExtractPropertyNameFromExpresssion(propertyLambda);
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static string ExtractPropertyNameFromExpresssion<T>(Expression<Func<T, object>> propertyLambda)
        {
            if (propertyLambda.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException("The expression must be a lambda");
            }

            MemberExpression funcBody = propertyLambda.Body as MemberExpression;
            if (funcBody == null)
            {
                throw new ArgumentException("the method body must be a member expression");                    
            }

            return funcBody.Member.Name;
        }
    }
}
