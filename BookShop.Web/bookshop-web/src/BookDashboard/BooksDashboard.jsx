import { useState, useEffect } from 'react';
import { responseError } from '../App/App';

export function BooksDashboard({ isAuthenticated })
{
    const [books, setBooks] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [name, setName] = useState('');
    const [price, setPrice] = useState('');
    const [saving, setSaving] = useState(false);
    const [orderingBookId, setOrderingBookId] = useState(null);
    const [orderMessage, setOrderMessage] = useState('');

    useEffect(() =>
    {
        let cancelled = false;

        async function loadBooks()
        {
            try
            {
                const response = await fetch('/api/book');
                if (!response.ok) throw new Error(await responseError(response));
                const result = await response.json();
                if (!cancelled) setBooks(result);
            } catch (loadError)
            {
                if (!cancelled) setError(loadError.message || 'Could not load books.');
            } finally
            {
                if (!cancelled) setLoading(false);
            }
        }

        loadBooks();
        return () => { cancelled = true; };
    }, []);

    async function addBook(event)
    {
        event.preventDefault();
        setSaving(true);
        setError('');

        try
        {
            const response = await fetch('/api/book', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${sessionStorage.getItem('bookshop.accessToken')}`,
                },
                body: JSON.stringify({ name, price: Number(price) }),
            });
            if (!response.ok) throw new Error(await responseError(response));

            const book = await response.json();
            setBooks((currentBooks) => [...currentBooks, book]);
            setName('');
            setPrice('');
        } catch (saveError)
        {
            setError(saveError.message || 'Could not add the book.');
        } finally
        {
            setSaving(false);
        }
    }

    async function createOrder(book)
    {
        setOrderingBookId(book.id);
        setOrderMessage('');
        setError('');

        try
        {
            const response = await fetch('/api/orders', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${sessionStorage.getItem('bookshop.accessToken')}`,
                },
                body: JSON.stringify({ bookId: book.id }),
            });
            if (!response.ok) throw new Error(await responseError(response));

            const order = await response.json();
            setOrderMessage(`Order #${order.id} created successfully.`);
        } catch (orderError)
        {
            setError(orderError.message || 'Could not create the order.');
        } finally
        {
            setOrderingBookId(null);
        }
    }

    return (
        <div className="dashboard-table-wrap">
            <h1>Books Dashboard</h1>
            {loading && <p>Loading books…</p>}
            {error && <p className="error" role="alert">{error}</p>}
            {orderMessage && <p role="status">{orderMessage}</p>}
            <table>
                <thead>
                    <tr>
                        <th>Book</th>
                        <th>Price</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>
                    {books.map((book) => (
                        <tr key={book.id}>
                            <td>{book.title} - {book.author?.name}</td>
                            <td>{Number(book.price).toLocaleString(undefined, { style: 'currency', currency: 'EUR' })}</td>
                            <td>
                               {/*  {isAuthenticated ? (
                                    <button
                                        type="button"
                                        onClick={() => createOrder(book)}
                                        disabled={orderingBookId !== null}
                                    >
                                        {orderingBookId === book.id ? 'Ordering…' : 'Order'}
                                    </button>
                                ) : 'Sign in to order'} */}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
            {!loading && books.length === 0 && <p>No books are currently listed.</p>}
        </div>
    );
}
