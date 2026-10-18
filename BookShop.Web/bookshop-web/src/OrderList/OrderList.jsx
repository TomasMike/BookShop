import { useState, useEffect } from 'react';
import { responseError } from '../LoginPanel/LoginPanel';

export function OrderList({ isAuthenticated })
{
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        let cancelled = false;

        async function loadOrders()
        {
            const accessToken = sessionStorage.getItem('bookshop.accessToken');

            if (!isAuthenticated || !accessToken)
            {
                setError('Sign in to view your orders.');
                setOrders([]);
                setLoading(false);
                return;
            }

            setLoading(true);
            setError('');
            try
            {
                const response = await fetch('/api/orders/customer', {
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                    },
                });
                if (!response.ok) throw new Error(await responseError(response));

                const result = await response.json();
                if (!cancelled) setOrders(result);
            }
            catch (loadError)
            {
                if (!cancelled) setError(loadError.message || 'Could not load your orders.');
            }
            finally
            {
                if (!cancelled) setLoading(false);
            }
        }

        loadOrders();
        return () => { cancelled = true; };
    }, [isAuthenticated]);

    return (
        <div>
            <h1>Order List</h1>
            {loading && <p>Loading orders…</p>}
            {error && <p className="error" role="alert">{error}</p>}
            {!loading && !error && orders.length === 0 && <p>You have no orders yet.</p>}
            {!loading && orders.length > 0 && (
                <table>
                    <thead>
                        <tr>
                            <th>Order</th>
                            <th>Book</th>
                            <th>Date</th>
                            <th>Status</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        {orders.map((order) => (
                            <tr key={order.id}>
                                <td>#{order.id}</td>
                                <td>#{order.bookId}</td>
                                <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                                <td>{order.status}</td>
                                <td>{Number(order.totalPrice).toLocaleString(undefined, { style: 'currency', currency: 'EUR' })}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}
