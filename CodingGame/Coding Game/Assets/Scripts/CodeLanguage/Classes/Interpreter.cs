public class Interpreter {
    public RTResult Visit(BinOpNode node, Context context) {
        return VisitBinOpNode(node, context);
    }

    public RTResult Visit(NumberNode node, Context context) {
        return VisitNumberNode(node, context);
    }

    public RTResult Visit(UnaryOpNode node, Context context) {
        /*Type type = node.GetType();
        string methodName = $"Visit{type}";
        MethodInfo method;
        method = GetType().GetMethod(methodName);
        if (method == null)
            Debug.LogError("No such method exists");
        return method.Invoke(type, new object[] { node });*/
        return VisitUnaryOpNode(node, context);
    }

    public RTResult Visit(VarAccessNode node, Context context) {
        return VisitVarAccessNode(node, context);
    }

    public RTResult Visit(VarAssignNode node, Context context) {
        return VisitVarAssignNode(node, context);
    }

    public RTResult VisitNumberNode(NumberNode node, Context context) {
        Number val = new Number(node.token.value).SetContext(context);
        val.SetPos(node.token.posStart, node.token.posEnd);
        RTResult r = new RTResult();
        r.Success(val);
        return r;
    }

    public RTResult VisitVarAccessNode(VarAccessNode node, Context context) {
        RTResult result = new RTResult();
        dynamic varName = node.varNameToken.value;
        Number value = context.symbolTable.Get(varName);

        if (value == null)
            return result.Failure(new RTError(node.posStart, node.posEnd, $"'{varName}' is not defined", context));

        value = value.Copy().SetPos(node.posStart, node.posEnd);
        return result.Success(value);
    }

    public RTResult VisitVarAssignNode(VarAssignNode node, Context context) {
        RTResult result = new RTResult();
        dynamic var_name = node.varNameToken.value;
        Number value = result.Register(Visit(node.valueNode, context));

        if (result.error != null)
            return result;

        context.symbolTable.Set(var_name, value);
        return result.Success(value);
    }

    public RTResult VisitBinOpNode(BinOpNode node, Context context) {
        RTResult res = new RTResult();
        Number left = null;
        Number right = null;

        if (node.leftNode != null) {
            left = res.Register(Visit(node.leftNode, context));
            if (res.error != null) return res; 
        }
        if (node.rightNode != null) {
            right = res.Register(Visit(node.rightNode, context));
            if (res.error != null) return res;
        }

        RTResult result = null;

        if (node.opToken == null)
            return new RTResult().Success(right);

        if (node.opToken.type == pseudo.TokenType.PLUS)
            result = left.AddedTo(right);
        else if (node.opToken.type == pseudo.TokenType.MINUS)
            result = left.SubBy(right);
        else if (node.opToken.type == pseudo.TokenType.MUL)
            result = left.MulBy(right);
        else if (node.opToken.type == pseudo.TokenType.DIV)
            result = left.DivBy(right);
        else if (node.opToken.type == pseudo.TokenType.POWER)
            result = left.PowBy(right);
        else if (node.opToken.type == pseudo.TokenType.EE)
            result = left.GetComparison_EQ(right);
        else if (node.opToken.type == pseudo.TokenType.NE)
            result = left.GetComparison_NE(right);
        else if (node.opToken.type == pseudo.TokenType.LT)
            result = left.GetComparison_LT(right);
        else if (node.opToken.type == pseudo.TokenType.GT)
            result = left.GetComparison_GT(right);
        else if (node.opToken.type == pseudo.TokenType.LTE)
            result = left.GetComparison_LTE(right);
        else if (node.opToken.type == pseudo.TokenType.GTE)
            result = left.GetComparison_GTE(right);
        else if (node.opToken.Matches(pseudo.TokenType.KEYWORD, "and"))
            result = left.AndedBy(right);
        else if (node.opToken.Matches(pseudo.TokenType.KEYWORD, "or"))
            result = left.OredBy(right);

        if (result.error != null)
            return res.Failure(result.error);

        return new RTResult().Success(result.value.SetPos(node.opToken.posStart, node.opToken.posEnd));
    }

    public RTResult VisitUnaryOpNode(UnaryOpNode node, Context context) {
        RTResult res = new RTResult();
        Number number = res.Register(Visit(node.node, context));
        if (res.error != null) return res;

        RTResult result = new RTResult();

        if (node.opToken.type == pseudo.TokenType.MINUS)
            result = number.MulBy(new Number(-1));
        else if (node.opToken.Matches(pseudo.TokenType.KEYWORD, "not"))
            result = number.Notted();
            
        if (result.error != null)
            return res.Failure(result.error);

        return res.Success(result.value.SetPos(node.opToken.posStart, node.opToken.posEnd));
    }

    public RTResult VisitIfNode(IfNode node, Context context) {
        RTResult result = new RTResult();

        foreach(Case c in node.cases) {
            Number conditionValue = result.Register(Visit(c.condition, context));

            if (result.error != null)
                return result;

            if (conditionValue.value != 0) {
                Number exprValue = result.Register(Visit(c.node, context));
                if (result.error != null)
                    return result;
                return result.Success(exprValue);
            }
        }

        if(node.elseCase) {
            Number exprValue = result.Register(Visit(node.elseCase, context));
            if (result.error != null)
                return result;
            return result.Success(exprValue);
        }

        return result.Success(null);
    }
}