
import { useEffect, useState } from 'react';
import { responseError } from '../api';

function Order({ orderId, onBack })
{
    const [order, setOrder] = useState(null);
    const [quantities, setQuantities] = useState({});
    const [loading, setLoading] = useState(true);
    const [savingDetailId, setSavingDetailId] = useState(null);
    const [error, setError] = useState('');

    useEffect(() => {
        let cancelled = false;

        async function loadOrder()
        {
            setLoading(true);
            setError('');
            try
            {
                const response = await fetch(`/api/orders/${orderId}`, {
                    headers: {
                        Authorization: `Bearer ${sessionStorage.getItem('bookshop.accessToken')}`,
                    },
                });
                if (!response.ok) throw new Error(await responseError(response));
                const result = await response.json();
                if (!cancelled)
                {
                    setOrder(result);
                    setQuantities(Object.fromEntries(result.details.map((detail) => [detail.id, detail.quantity])));
                }
            }
            catch (loadError)
            {
                if (!cancelled) setError(loadError.message || 'Could not load the order.');
            }
            finally
            {
                if (!cancelled) setLoading(false);
            }
        }

        loadOrder();
        return () => { cancelled = true; };
    }, [orderId]);

    async function updateQuantity(detailId)
    {
        setSavingDetailId(detailId);
        setError('');
        try
        {
            const response = await fetch(`/api/orders/${orderId}/details/${detailId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${sessionStorage.getItem('bookshop.accessToken')}`,
                },
                body: JSON.stringify({ quantity: Number(quantities[detailId]) }),
            });
            if (!response.ok) throw new Error(await responseError(response));
            setOrder(await response.json());
        }
        catch (saveError)
        {
            setError(saveError.message || 'Could not update the quantity.');
        }
        finally
        {
            setSavingDetailId(null);
        }
    }

    async function removeBook(detailId)
    {
        setSavingDetailId(detailId);
        setError('');
        try
        {
            const response = await fetch(`/api/orders/${orderId}/details/${detailId}`, {
                method: 'DELETE',
                headers: {
                    Authorization: `Bearer ${sessionStorage.getItem('bookshop.accessToken')}`,
                },
            });
            if (!response.ok) throw new Error(await responseError(response));
            const updatedOrder = await response.json();
            setOrder(updatedOrder);
            setQuantities(Object.fromEntries(updatedOrder.details.map((detail) => [detail.id, detail.quantity])));
        }
        catch (removeError)
        {
            setError(removeError.message || 'Could not remove the book.');
        }
        finally
        {
            setSavingDetailId(null);
        }
    }

    return (
        <div className="order-detail">
            <button type="button" onClick={onBack}>Back to books and orders</button>
            <h1>Order #{orderId}</h1>
            {loading && <p>Loading order…</p>}
            {error && <p className="error" role="alert">{error}</p>}
            {!loading && order && (
                <>
                    <p>Status: {order.status}</p>
                    {order.details.length === 0 ? <p>This order has no books.</p> : (
                        <table>
                            <thead>
                                <tr>
                                    <th>Book</th>
                                    <th>Unit price</th>
                                    <th>Quantity</th>
                                    <th>Total</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                {order.details.map((detail) => (
                                    <tr key={detail.id}>
                                        <td>{detail.bookTitle} - {detail.authorName}</td>
                                        <td>{Number(detail.unitPrice).toLocaleString(undefined, { style: 'currency', currency: 'EUR' })}</td>
                                        <td>
                                            <input
                                                type="number"
                                                min="1"
                                                value={quantities[detail.id] ?? detail.quantity}
                                                onChange={(event) => setQuantities((current) => ({ ...current, [detail.id]: event.target.value }))}
                                                aria-label={`Quantity for ${detail.bookTitle}`}
                                            />
                                            <button
                                                type="button"
                                                onClick={() => updateQuantity(detail.id)}
                                                disabled={savingDetailId !== null}
                                            >
                                                {savingDetailId === detail.id ? 'Saving…' : 'Update'}
                                            </button>
                                        </td>
                                        <td>{Number(detail.lineTotal).toLocaleString(undefined, { style: 'currency', currency: 'EUR' })}</td>
                                        <td>
                                            <button type="button" onClick={() => removeBook(detail.id)} disabled={savingDetailId !== null}>
                                                Remove
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                    <p><strong>Order total:</strong> {Number(order.totalPrice).toLocaleString(undefined, { style: 'currency', currency: 'EUR' })}</p>
                </>
            )}
        </div>
    );
}

export default Order;
