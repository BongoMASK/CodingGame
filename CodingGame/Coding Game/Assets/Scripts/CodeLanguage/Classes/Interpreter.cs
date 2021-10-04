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

    public RTResult VisitNumberNode(NumberNode node, Context context) {
        Number val = new Number(node.token.value).SetContext(context);
        val.SetPos(node.token.posStart, node.token.posEnd);
        RTResult r = new RTResult();
        r.Success(val);
        return r;
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
        if (node.opToken.type == pseudo.TokenType.MINUS)
            result = left.SubBy(right);
        if (node.opToken.type == pseudo.TokenType.MUL)
            result = left.MulBy(right);
        if (node.opToken.type == pseudo.TokenType.DIV)
            result = left.DivBy(right);
        if (node.opToken.type == pseudo.TokenType.POWER) 
            result = left.PowBy(right);

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

        if (result.error != null)
            return res.Failure(result.error);

        return res.Success(result.value.SetPos(node.opToken.posStart, node.opToken.posEnd));
    }
}