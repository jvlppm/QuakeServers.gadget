using System;
using Quake2.Variables;

namespace Quake2.Client.Events
{
	public class ValueChangedEventArgs<ValueType> : EventArgs
	{
		public ValueChangedEventArgs(ValueType oldValue, ValueType newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		public ValueType NewValue { get; private set; }
		public ValueType OldValue { get; private set; }
	}

	public class VariableValueChangedEventArgs : ValueChangedEventArgs<object>
	{
		public VariableValueChangedEventArgs(Variable variable, object oldValue, object newValue)
			: base(oldValue, newValue)
		{
			Variable = variable;
		}

		public Variable Variable { get; private set; }
	}
	public delegate void ValueChangedEventHandler<Type>(object sender, ValueChangedEventArgs<Type> e);
	public delegate object VariableValueChangedEventHandler(object sender, VariableValueChangedEventArgs e);
}
