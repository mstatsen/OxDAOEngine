using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Filter.Types
{
    public class FilterOperationHelper : AbstractTypeHelper<FilterOperation>
    {
        public override FilterOperation EmptyValue() =>
            FilterOperation.Equals;

        public override string GetName(FilterOperation value) =>
            value switch
            {
                FilterOperation.Equals => "Equals",
                FilterOperation.NotEquals => "Not Equals",
                FilterOperation.Contains => "Contains",
                FilterOperation.StartsWith => "Starts with",
                FilterOperation.EndsWith => "Ends with",
                FilterOperation.NotContains => "Not Contains",
                FilterOperation.Greater => "Greater",
                FilterOperation.Lower => "Lower",
                FilterOperation.Blank => "Blank",
                FilterOperation.NotBlank => "Not Blank",
                _ => string.Empty,
            };

        public bool IsUnaryOperation(FilterOperation operation) =>
            FilterOperations.UnaryOperations.Contains(operation);

        public bool Match(FilterOperation operation, object? leftObject, object? rightObject)
        {
            if ((leftObject is IEmptyChecked lec && lec.IsEmpty)
                || (rightObject is IEmptyChecked rec && rec.IsEmpty))
                return true;

            string? leftString = leftObject is DAO leftDao ? leftDao.MatchingString() : leftObject?.ToString();
            string? rightString = rightObject is DAO rightDao ? rightDao.MatchingString() : rightObject?.ToString();

            return operation switch
            {
                FilterOperation.Equals =>
                    leftObject != null && leftObject.Equals(rightObject),
                FilterOperation.NotEquals =>
                    !Match(FilterOperation.Equals, leftObject, rightObject),
                FilterOperation.Contains =>
                    leftString != null && leftString != string.Empty
                        && rightString != null && rightString != string.Empty
                        && leftString.ToUpper().Contains(rightString.ToUpper()),
                FilterOperation.NotContains =>
                    !Match(FilterOperation.Contains, leftObject, rightObject),
                FilterOperation.StartsWith =>
                    leftString != null && leftString != string.Empty
                        && rightString != null && rightString != string.Empty
                        && leftString.ToUpper().StartsWith(rightString.ToUpper()),
                FilterOperation.EndsWith =>
                    leftString != null && leftString != string.Empty
                        && rightString != null && rightString != string.Empty
                        && leftString.ToUpper().EndsWith(rightString.ToUpper()),
                FilterOperation.Greater =>
                    DAO.IntValue(leftObject) > DAO.IntValue(rightObject),
                FilterOperation.Lower =>
                    DAO.IntValue(leftObject) < DAO.IntValue(rightObject),
                FilterOperation.Blank =>
                    leftObject == null || leftString == null || leftString == string.Empty,
                FilterOperation.NotBlank =>
                    !Match(FilterOperation.Blank, leftObject, rightObject),
                _ => true,
            };
        }
    }
}