﻿import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './LogInPanel.css';

export const LogInPanel = () => {
    const [name, setName] = useState('');
    const [surname, setSurname] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [repeatPassword, setRepeatPassword] = useState('');

    const navigate = useNavigate();
    const [showDialog, setShowDialog] = useState(false);
    const [targetRoute, setTargetRoute] = useState('');

    useEffect(() => {
        const numberOfStars = 100;
        const stars = [];

        for (let i = 0; i < numberOfStars; i++) {
            const star = document.createElement('div');
            star.classList.add('star');

            star.style.top = `${Math.random() * 100}vh`;
            star.style.left = `${Math.random() * 100}vw`;

            const angle = Math.random() * 2 * Math.PI;
            star.style.setProperty('--dx', Math.cos(angle));
            star.style.setProperty('--dy', Math.sin(angle));

            star.style.animationDuration = `${2 + Math.random() * 5}s`;

            document.body.appendChild(star);
            stars.push(star); 
        }

        return () => {
            stars.forEach(star => document.body.removeChild(star));
        };
    }, []);

    const handleLoginSubmit = async (e) => {
        try {
            e.preventDefault() 

            const requestBody = JSON.stringify({email, password})
            const response = await fetch('https://localhost:7283/User/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: requestBody
            })

            if (!response.ok) {
                const data = await response.json()
                return
            }
            
            navigate(targetRoute);
        } catch(error) {
            console.error(error)
        }
    }

    const handleRegisterSubmit = async (e) => {
        try {
            e.preventDefault();

            if (password !== repeatPassword) {
                return;
            }

            const requestBody = JSON.stringify({ name, surname, email, password })
            const response = await fetch('https://localhost:7283/User/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include',
                body: requestBody
            });

            if (!response.ok) {
                const data = await response.json()
                return
            }
            
            navigate(targetRoute)
        } catch (error) {
            console.error(error)
        }
    }
    const showLoginDialog = (route) => {
        setTargetRoute(route);
        setShowDialog(true);
    };

    return (
        <div className="container">
            <h1 className="edu">EduPal</h1>
            <span className="buttons">
        <button onClick={() => showLoginDialog('/Subjects')}>START LEARNING!</button>
            </span>
            {showDialog &&
                <div className="dialog">
                    <div className="dialog-content">
                        <div className="form-section">
                            <h1 className="form-name">Log In</h1>
                            <form onSubmit={handleLoginSubmit} autoComplete={"on"}>
                                <input type="email" placeholder="Email" required onChange={e => setEmail(e.target.value)} />
                                <input type="password" placeholder="Password" required onChange={e => setPassword(e.target.value)} />
                                <button type="submit">Log In</button>
                            </form>
                        </div>
                        <div className="form-section">
                            <h1 className="form-name">Register</h1>
                            <form onSubmit={handleRegisterSubmit} autoComplete={"on"}>
                                <input type="text" placeholder="Name" required onChange={e => setName(e.target.value)} />
                                <input type="text" placeholder="Surname" required onChange={e => setSurname(e.target.value)} />
                                <input type="email" placeholder="Email" required onChange={e => setEmail(e.target.value)} />
                                <input type="password" placeholder="Password" required onChange={e => setPassword(e.target.value)} />
                                <input type="password" placeholder="Repeat Password" required onChange={e => setRepeatPassword(e.target.value)} />
                                <button type="submit">Register</button>
                            </form>
                        </div>
                    </div>
                    <div className="dialog-footer">
                        <button type="button" onClick={() => setShowDialog(false)}>Cancel</button>
                    </div>
                </div>
            }
        </div>
    );
};
