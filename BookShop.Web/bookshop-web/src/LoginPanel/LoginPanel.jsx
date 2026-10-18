import { useState } from 'react';

export function LoginPanel({ onAuthChange })
{
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');
    const [signedIn, setSignedIn] = useState(() => Boolean(sessionStorage.getItem('bookshop.accessToken')));
    const [busy, setBusy] = useState(false);

    async function loginSubmit()
    {
        setBusy(true);
        setMessage('');

        try
        {
            const response = await fetch('/api/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password }),
            });
            if (!response.ok) throw new Error(await responseError(response));

            const result = await response.json();
            sessionStorage.setItem('bookshop.accessToken', result.accessToken);
            sessionStorage.setItem('bookshop.email', email);
            setSignedIn(true);
            onAuthChange(true);

            const inforesponse = await fetch(`/api/customer/email/${email}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${sessionStorage.getItem('bookshop.accessToken')}`,
                }
            });
            if (!inforesponse.ok) throw new Error(await responseError(inforesponse));





            const inf = await inforesponse.json();
            sessionStorage.setItem('bookshop.id', inf.id);
            sessionStorage.setItem('bookshop.userId', inf.userId);
            



        }
        catch (error)
        {
            setMessage(error.message || 'Unable to connect to the server.');
        }
        finally
        {
            setBusy(false);
        }

    }

    async function registerSubmit()
    {
        setBusy(true);
        setMessage('');

        try
        {
            const regResponse = await fetch('/api/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password }),
            });
            if (!regResponse.ok) throw new Error(await responseError(regResponse));

            const response = await fetch('/api/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password }),
            });
            if (!response.ok) throw new Error(await responseError(response));

            const result = await response.json();
            sessionStorage.setItem('bookshop.accessToken', result.accessToken);
            sessionStorage.setItem('bookshop.email', email);
            setSignedIn(true);
            onAuthChange(true);
        }
        catch (error)
        {
            setMessage(error.message || 'Unable to connect to the server.');
        }
        finally
        {
            setBusy(false);
        }
    }

    function submitForm(event)
    {
        event.preventDefault();
        loginSubmit();
    }

    function submitAction(event, action)
    {
        if (!event.currentTarget.form.reportValidity())
        {
            return;
        }

        if (action === 'register')
        {
            registerSubmit();
            return;
        }

        loginSubmit();
    }

    function signOut()
    {
        sessionStorage.removeItem('bookshop.accessToken');
        sessionStorage.removeItem('bookshop.email');
        setSignedIn(false);
        onAuthChange(false);
        setPassword('');
    }

    return (
        <>
            <div className="login-page">
                {signedIn ? (
                    <div>Signed in as {sessionStorage.getItem('bookshop.email')}<button onClick={signOut}>Sign out</button></div>
                ) : (
                    <>
                        <div>Not logged in</div>
                        <form onSubmit={submitForm}>
                            <div>
                                <label htmlFor="email">Email</label>
                                <input
                                    id="email"
                                    type="email"
                                    value={email}
                                    onChange={(event) => setEmail(event.target.value)}
                                    required
                                />

                                <label htmlFor="password">Password</label>
                                <input
                                    id="password"
                                    type="password"
                                    value={password}
                                    onChange={(event) => setPassword(event.target.value)}
                                    required
                                />
                            </div>
                            <div>
                                <button
                                    className="primary-button"
                                    type="button"
                                    onClick={(event) => submitAction(event, 'login')}
                                    disabled={busy}
                                >
                                    {busy ? 'Please wait…' : 'Sign in'}
                                </button>
                                <button
                                    className="primary-button"
                                    type="button"
                                    onClick={(event) => submitAction(event, 'register')}
                                    disabled={busy}
                                >
                                    {busy ? 'Please wait…' : 'Create Account'}
                                </button>
                            </div>
                        </form>
                        {message && <p className="error" role="alert">{message}</p>}
                    </>
                )}
            </div>
            <p></p>
        </>
    );
}

export async function responseError(response)
{
    try
    {
        const body = await response.json();
        return body.error || body.detail || body.title || Object.values(body.errors || {}).flat().join(' ') || 'Sign-in failed.';
    }
    catch
    {
        return 'Sign-in failed.';
    }
}

export default LoginPanel;
