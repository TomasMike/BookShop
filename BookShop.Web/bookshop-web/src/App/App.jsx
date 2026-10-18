import './App.css';
import { useState } from 'react';
import { BooksDashboard } from '../BookDashboard/BooksDashboard';
import { LoginPanel, responseError } from '../LoginPanel/LoginPanel';
import { OrderList } from '../OrderList/OrderList';

function App() {
    const [pageState, setPageState] = useState('dashboard');
    const [isAuthenticated, setIsAuthenticated] = useState(() => Boolean(sessionStorage.getItem('bookshop.accessToken')));

    const renderPage = () => {
        switch (pageState) {
            case 'dashboard':
                return <>
                    <BooksDashboard isAuthenticated={isAuthenticated} />
                    <OrderList isAuthenticated={isAuthenticated} />
                    
                </>
            default:
                return <OrderList isAuthenticated={isAuthenticated} />;
        }
    };

    return (
        <div className="app-shell">
            <LoginPanel onAuthChange={setIsAuthenticated} />

            <div className="dashboard-section">
                {renderPage()}
            </div>
        </div>
    );
}

export { responseError };
export default App;
