import { useEffect, useState } from 'react';
import { responseError } from '../api';

export function BooksDashboard({ isAuthenticated, onOpenOrder })
{
    const [books, setBooks] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [orderingBookId, setOrderingBookId] = useState(null);

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

    async function addToOrder(book)
    {
        setOrderingBookId(book.id);
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
            onOpenOrder(order.id);
        } catch (orderError)
        {
            setError(orderError.message || 'Could not add the book to an order.');
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
                                {isAuthenticated && (
                                    <button
                                        type="button"
                                        onClick={() => addToOrder(book)}
                                        disabled={orderingBookId !== null || !book.isAvailable}
                                    >
                                        {orderingBookId === book.id ? 'Adding…' : book.isAvailable ? 'Add to order' : 'Unavailable'}
                                    </button>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
            {!loading && books.length === 0 && <p>No books are currently listed.</p>}
        </div>
    );
}
