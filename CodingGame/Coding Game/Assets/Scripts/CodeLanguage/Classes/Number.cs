using System;

public class Number {
    public dynamic value;
    public Context context;
    public Error error;
    public Position posStart;
    public Position posEnd;

    public Number(dynamic value) {
        this.value = value;
        SetPos();
        SetContext();
    }

    public Number SetPos(Position posStart = null, Position posEnd = null) {
        this.posStart = posStart;
        this.posEnd = posEnd;
        return this;
    }

    public Number SetContext(Context context = null) {
        this.context = context;
        return this;
    }

    public RTResult AddedTo(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value + other.value).SetContext(context), null);

        return new RTResult(this, null);
    }

    public RTResult SubBy(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value - other.value).SetContext(context), null);

        return new RTResult(this, null);
    }

    public RTResult MulBy(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value * other.value).SetContext(context), null);

        return new RTResult(this, null);
    }

    public RTResult DivBy(dynamic other) {
        if (other.value == 0)
            return new RTResult(null, new RTError(other.posStart, other.posEnd, "Division by zero", context));

        if (other.GetType() == GetType())
            return new RTResult(new Number(value / other.value).SetContext(context), null);

        return new RTResult(this, null);
    }

    public RTResult PowBy(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(Math.Pow(value, other.value)).SetContext(context), null);

        return new RTResult(this, null);
    }

    public RTResult GetComparison_EQ(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value == other.value).SetContext(context), null);
        return new RTResult(this, null);
    }

    public RTResult GetComparison_NE(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value != other.value).SetContext(context), null);
        return new RTResult(this, null);
    }

    public RTResult GetComparison_LT(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value < other.value).SetContext(context), null);
        return new RTResult(this, null);
    }

    public RTResult GetComparison_GT(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value > other.value).SetContext(context), null);
        return new RTResult(this, null);
    }

    public RTResult GetComparison_LTE(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value <= other.value).SetContext(context), null);
        return new RTResult(this, null);
    }

    public RTResult GetComparison_GTE(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value >= other.value).SetContext(context), null);
        return new RTResult(this, null);
    }

    public RTResult AndedBy(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value && other.value).SetContext(context), null);
        return new RTResult(this, null);
    }
    public RTResult OredBy(dynamic other) {
        if (other.GetType() == GetType())
            return new RTResult(new Number(value || other.value).SetContext(context), null);
        return new RTResult(this, null);
    }

    public RTResult Notted() {
        int x = value == 0 ? 1 : 0;
        return new RTResult(new Number(x).SetContext(context), null);
    }

    public Number Copy() {
        Number copy = new Number(value);
        copy.SetPos(posStart, posEnd);
        copy.SetContext(context);
        return copy;
    }

    public string Display() {
        return value.ToString();
    }

}
