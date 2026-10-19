import './App.css';
import { useState } from 'react';
import { BooksDashboard } from '../BookDashboard/BooksDashboard';
import { LoginPanel } from '../LoginPanel/LoginPanel';
import { OrderList } from '../OrderList/OrderList';
import Order from '../Order/Order';

function App() {
    const [pageState, setPageState] = useState('dashboard');
    const [selectedOrderId, setSelectedOrderId] = useState(null);
    const [isAuthenticated, setIsAuthenticated] = useState(() => Boolean(sessionStorage.getItem('bookshop.accessToken')));

    function openOrder(orderId)
    {
        setSelectedOrderId(orderId);
        setPageState('order');
    }

    function handleAuthChange(authenticated)
    {
        setIsAuthenticated(authenticated);
        if (!authenticated) setPageState('dashboard');
    }

    const renderPage = () => {
        switch (pageState) {
            case 'dashboard':
                return <>
                    <BooksDashboard isAuthenticated={isAuthenticated} onOpenOrder={openOrder} />
                    <OrderList isAuthenticated={isAuthenticated} onOpenOrder={openOrder} />
                </>
            case 'order':
                return <Order orderId={selectedOrderId} onBack={() => setPageState('dashboard')} />;
            default:
                return null;
        }
    };

    return (
        <div className="app-shell">
            <LoginPanel onAuthChange={handleAuthChange} />

            <div className="dashboard-section">
                {renderPage()}
            </div>
        </div>
    );
}

export default App;
