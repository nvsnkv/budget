using System.Linq.Expressions;
using System.Reflection;
using AutoFixture.Kernel;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Utilities.Testing;

public class ReadableExpressionsBuilder : ISpecimenBuilder
{
    private readonly Dictionary<Type, object> _createdObjects = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not Type typedRequest)
        {
            return new NoSpecimen();
        }

        if (_createdObjects.TryGetValue(typedRequest, out var value))
        {
            return value;
        }

        if (typedRequest.IsGenericType && typedRequest.GetGenericTypeDefinition() == typeof(ReadableExpression<>))
        {
            var expressionType = typedRequest.GetGenericArguments().Single();
            if (TryBuildExpression(expressionType, out var represntation, out var expression))
            {
                value = Activator.CreateInstance(typedRequest, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, [represntation, expression], null, null)
                    ?? throw new InvalidOperationException("Unable to create ReadableExpression!");
                _createdObjects[typedRequest] = value;

                return value;
            }
        }

        return new NoSpecimen();
    }

    private bool TryBuildExpression(Type expressionType, out string representation, out Expression expression)
    {

        if (expressionType.IsGenericType && expressionType.GetGenericTypeDefinition() == typeof(Func<,>))
        {
            var parms = expressionType.GetGenericArguments();
            var argType = parms[0];
            var retVal = parms[1];



            var (rep, constant) = CreateConstant(retVal);
            expression = Expression.Lambda(
                Expression.Constant(constant, retVal),
                false,
                Expression.Parameter(argType));

            representation = "_ " + rep;

            return true;
        }

        if (expressionType.IsGenericType && expressionType.GetGenericTypeDefinition() == typeof(Func<,,>))
        {
            var parms = expressionType.GetGenericArguments();
            var firstArg = parms[0];
            var secondArg = parms[1];
            var retVal = parms[2];

            var (rep, constant) = CreateConstant(retVal);
            expression = Expression.Lambda(
                Expression.Constant(constant, retVal),
                false,
                Expression.Parameter(firstArg),
                Expression.Parameter(secondArg));

            representation = "(_,__) " + rep;

            return true;
        }

        representation = "";
        expression = Expression.Constant(null);

        return false;
    }

    private (string, object?) CreateConstant(Type retVal)
    {
        if (retVal == typeof(bool))
        {
            return ("=> false", false);
        }

        if (retVal == typeof(string))
        {
            return ("=> \"\"", string.Empty);
        }

        return ("=> null", null);
    }
}
