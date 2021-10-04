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

    public string Display() {
        return value.ToString();
    }

}
