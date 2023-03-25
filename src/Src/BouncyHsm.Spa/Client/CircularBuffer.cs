using System.Collections;

namespace BouncyHsm.Spa.Client;

public class CircularBuffer<T> : IEnumerable<T>
{
    private readonly T[] buffer;
    private int start;
    private int end;
    private int size;

    public int Capacity
    {
        get => this.buffer.Length;
    }
    public bool IsFull
    {
        get
        {
            return this.size == this.Capacity;
        }
    }

    public bool IsEmpty
    {
        get => this.size == 0;
    }

    public int Size
    {
        get => this.size;
    }

    public T this[int index]
    {
        get
        {
            if (this.IsEmpty)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));
            }
            if (index >= this.size)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, this.size));
            }

            int actualIndex = this.InternalIndex(index);
            return this.buffer[actualIndex];
        }
        set
        {
            if (this.IsEmpty)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));
            }
            if (index >= this.size)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, this.size));
            }

            int actualIndex = this.InternalIndex(index);
            this.buffer[actualIndex] = value;
        }
    }


    public CircularBuffer(int capacity)
           : this(capacity, new Memory<T>())
    {
    }

    public CircularBuffer(int capacity, Memory<T> items)
    {
        if (capacity < 1)
        {
            throw new ArgumentException(
                "Circular buffer cannot have negative or zero capacity.", nameof(capacity));
        }
        
        if (items.Length > capacity)
        {
            throw new ArgumentException(
                "Too many items to fit circular buffer", nameof(items));
        }

        this.buffer = new T[capacity];
        items.CopyTo(this.buffer.AsMemory());
        this.size = items.Length;

        this.start = 0;
        this.end = this.size == capacity ? 0 : this.size;
    }

    public T Front()
    {
        this.ThrowIfEmpty();
        return this.buffer[this.start];
    }

    public T Back()
    {
        this.ThrowIfEmpty();
        return this.buffer[(this.end != 0 ? this.end : this.Capacity) - 1];
    }

    public void PushBack(T item)
    {
        if (this.IsFull)
        {
            this.buffer[this.end] = item;
            this.Increment(ref this.end);
            this.start = this.end;
        }
        else
        {
            this.buffer[this.end] = item;
            this.Increment(ref this.end);
            ++this.size;
        }
    }

    public void PushFront(T item)
    {
        if (this.IsFull)
        {
            this.Decrement(ref this.start);
            this.end = this.start;
            this.buffer[this.start] = item;
        }
        else
        {
            this.Decrement(ref this.start);
            this.buffer[this.start] = item;
            ++this.size;
        }
    }

    public void PopBack()
    {
        this.ThrowIfEmpty("Cannot take elements from an empty buffer.");
        this.Decrement(ref this.end);
        this.buffer[this.end] = default!;
        --this.size;
    }

    public void PopFront()
    {
        this.ThrowIfEmpty("Cannot take elements from an empty buffer.");
        this.buffer[this.start] = default!;
        this.Increment(ref this.start);
        --this.size;
    }

    public void Clear()
    {
        this.start = 0;
        this.end = 0;
        this.size = 0;

        if (!typeof(T).IsValueType)
        {
            Array.Clear(this.buffer, 0, this.buffer.Length);
        }
    }

    public T[] ToArray()
    {
        T[] newArray = new T[this.Size];
        Memory<T> arrayOne = this.ArrayOne();
        Memory<T> arrayTwo = this.ArrayTwo();

        arrayOne.CopyTo(newArray.AsMemory(0, arrayOne.Length));
        arrayTwo.CopyTo(newArray.AsMemory(arrayOne.Length, arrayOne.Length));

        return newArray;
    }

    public IEnumerator<T> GetEnumerator()
    {
        Memory<T> arrayOne = this.ArrayOne();
        Memory<T> arrayTwo = this.ArrayTwo();

        for (int i = 0; i < arrayOne.Length; i++)
        {
            yield return arrayOne.Span[i];
        }

        for (int i = 0; i < arrayTwo.Length; i++)
        {
            yield return arrayTwo.Span[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    private void ThrowIfEmpty(string message)
    {
        if (this.IsEmpty)
        {
            throw new InvalidOperationException(message);
        }
    }

    private void ThrowIfEmpty()
    {
        if (this.IsEmpty)
        {
            throw new InvalidOperationException("Cannot access an empty buffer.");
        }
    }

    private void Increment(ref int index)
    {
        if (++index == this.Capacity)
        {
            index = 0;
        }
    }

    private void Decrement(ref int index)
    {
        if (index == 0)
        {
            index = this.Capacity;
        }

        index--;
    }

    private int InternalIndex(int index)
    {
        return this.start + (index < (this.Capacity - this.start) ? index : index - this.Capacity);
    }

    private Memory<T> ArrayOne()
    {
        if (this.IsEmpty)
        {
            return new Memory<T>();
        }
        else if (this.start < this.end)
        {
            return new Memory<T>(this.buffer, this.start, this.end - this.start);
        }
        else
        {
            return new Memory<T>(this.buffer, this.start, this.buffer.Length - this.start);
        }
    }

    private Memory<T> ArrayTwo()
    {
        if (this.IsEmpty)
        {
            return new Memory<T>();
        }
        else if (this.start < this.end)
        {
            return new Memory<T>(this.buffer, this.end, 0);
        }
        else
        {
            return new Memory<T>(this.buffer, 0, this.end);
        }
    }
}
