# Mitchell's Logic Gates API

## An Open Source Subset of Mitchell's Boolean Logic Puzzle Maker



Code by Mitchell Scott Lampert  Mitch@MitchLampert.net



An open source implementation of a programming interface for working with components of logic circuits. The API provides basic logic components, and a virtual circuit board to wire them up into. Coded in C# 10.



This a subset of the code behind a "Boolean Logic Puzzle Maker" currently in development. For this implementation, only very basic components are provided, and all of the game-related features (such as winning goals and restriction rules) have been removed.



The Solution folder contains two projects. One is the **MSLLogicGatesAPILib**, which will be the focus of this document. The other is a **MSLLogicGatesSampleClient**, which offers a basic example for how to render the objects in the API onto a screen.



**Make sure the MSLLogicGatesSample*Client* is set as the Startup Project for the solution, when you run it!**



Read the main document, **Mitchells Logic Gates API Manual.docx**, for details.





## Requirements

All code was written with C#/.NET 10.0. It has been tested to work on Microsoft Windows 10 and 11.



The sample client is a Windows Presentation Foundation (WPF) application project, and its "Logic Board Control" makes use of GDI+. (I know GDI+ is old, but much of this code pre-dates newer things.)



It does NOT have any third-party dependencies.

