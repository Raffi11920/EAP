using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.RMSDriver.RecValidationHandling
{
    using EAP.Base.BridgeMessage.Common;
    using EAP.Utilities.ExtensionPlug;

    public class ValidationHandling
    {
        #region Private Field

        private delegate string range(float min, float max, float value);
        private delegate string other(float rules, float value);
        private delegate string contain(string rules, string value);
        private Dictionary<RecParameterValidation.EnumValidationRule, Delegate> mDic;

        #endregion

        #region Public Properties

        public Dictionary<RecParameterValidation.EnumValidationRule, Delegate> ValidationRules
        {
            get { return mDic; }
        }

        #endregion

        #region Constructor

        public ValidationHandling()
        {
            Initialize();
        }

        #endregion

        #region Private Method

        private void Initialize()
        {
            range methodRange = new range(Range);
            contain methodEqual = new contain(Equal);
            other methodLessThan = new other(LessThan);
            other methodMoreThan = new other(MoreThan);
            other methodLessThanEqual = new other(LessThanEqual);
            other methodMoreThanEqual = new other(MoreThanEqual);
            contain methodContain = new contain(Contain);
            contain methodNotContain = new contain(NotContain);

            mDic = new Dictionary<RecParameterValidation.EnumValidationRule, Delegate>();
            mDic.Add(RecParameterValidation.EnumValidationRule.Range, methodRange);
            mDic.Add(RecParameterValidation.EnumValidationRule.Equal, methodEqual);
            mDic.Add(RecParameterValidation.EnumValidationRule.LessThan, methodLessThan);
            mDic.Add(RecParameterValidation.EnumValidationRule.MoreThan, methodMoreThan);
            mDic.Add(RecParameterValidation.EnumValidationRule.LessThanEqual, methodLessThanEqual);
            mDic.Add(RecParameterValidation.EnumValidationRule.MoreThanEqual, methodMoreThanEqual);
            mDic.Add(RecParameterValidation.EnumValidationRule.Contains, methodContain);
            mDic.Add(RecParameterValidation.EnumValidationRule.NotContains, methodNotContain);
        }

        private string Range(float min, float max, float value)
        {
            if (value < min)
            {
                return ValidationText.RANGE.FillArguments(value, min, max);
            }
            else if (value > max)
            {
                return ValidationText.RANGE.FillArguments(value, min, max);
            }
            else
            {
                return ValidationText.OK;
            }
        }

        private string Equal(string rules, string value)
        {
            if (value != rules)
            {
                return ValidationText.NOTEQUAL.FillArguments(value, rules);
            }
            else
            {
                return ValidationText.OK;
            }
        }

        private string LessThan(float rules, float value)
        {
            if (value > rules)
            {
                return ValidationText.MORETHAN.FillArguments(value, rules);
            }
            else if (value == rules)
            {
                return ValidationText.EQUAL.FillArguments(value, rules);
            }
            else
            {
                return ValidationText.OK;
            }
        }

        private string MoreThan(float rules, float value)
        {
            if (value < rules)
            {
                return ValidationText.LESSTHAN.FillArguments(value, rules);
            }
            else if (value == rules)
            {
                return ValidationText.EQUAL.FillArguments(value, rules);
            }
            else
            {
                return ValidationText.OK;
            }
        }

        private string LessThanEqual(float rules, float value)
        {
            if (value > rules)
            {
                return ValidationText.MORETHAN.FillArguments(value, rules);
            }
            else
            {
                return ValidationText.OK;
            }
        }

        private string MoreThanEqual(float rules, float value)
        {
            if (value < rules)
            {
                return ValidationText.LESSTHAN.FillArguments(value, rules);
            }
            else
            {
                return ValidationText.OK;
            }
        }

        private string Contain(string rules, string value)
        {
            if (!rules.Contains(value))
            {
                return ValidationText.NOTCONTAIN.FillArguments(value, rules);
            }
            else
            {
                return ValidationText.OK;
            }
        }

        private string NotContain(string rules, string value)
        {
            if (rules.Contains(value))
            {
                return ValidationText.CONTAIN.FillArguments(value, rules);
            }
            else
            {
                return ValidationText.OK;
            }
        }

        #endregion
    }
}
