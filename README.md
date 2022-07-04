# KorbitSideShiftCryptoConverter

[![.NET](https://github.com/ipasechnikov/KorbitSideShiftCryptoConverter/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ipasechnikov/KorbitSideShiftCryptoConverter/actions/workflows/dotnet.yml)

A small console application to help you find best conversion rates for your crypto bought on [Korbit](https://www.korbit.co.kr/) exchange and convert it through [sideshift.ai](https://sideshift.ai/) exchange.

Just in case if someone is worried, this app doesn't collect any data, it cannot use your money to buy or sell any crypto. It simply converts KRW into target crypto with a help of intermediate crypto. Nothing more, nothing less.

## Table of Contents

- [KorbitSideShiftCryptoConverter](#korbitsideshiftcryptoconverter)
  - [Table of Contents](#table-of-contents)
  - [⚠️ Disclaimer](#️-disclaimer)
  - [Foreword](#foreword)
    - [Why Korbit as main exchange](#why-korbit-as-main-exchange)
    - [Why sideshift.ai as exchange for conversion](#why-sideshiftai-as-exchange-for-conversion)
  - [Preview](#preview)
  - [Getting started](#getting-started)
    - [Binaries](#binaries)
    - [Building](#building)
    - [Running](#running)
    - [Available coins](#available-coins)
    - [Modifying](#modifying)
  - [Contributing](#contributing)
  - [License](#license)

## ⚠️ Disclaimer

**I don't advise to buy or invest in crypto. I'm not a financial advisor. And for God's sake, please don't think that this app is supposed to make you rich or something. It's just a simple conversion tool that tries to minimize withdrawal fees for your crypto. It might be not the most accurate tool and there might be bugs. Double check actual conversion rates on exchanges.**

**USE IT ON YOUR OWN RISK!**

## Foreword

Just a small introduction why this tool was created.

[Korbit](https://www.korbit.co.kr/) exchange has quite high fees when you withdraw crypto, especially for BTC.
To withdraw BTC you have to pay 0.001 BTC (as for 2022-07-01). And we all want to withdraw our crypto from exchange as quickly as possible. Because as you know **not your keys not your coins**. But paying 0.001 BTC every time you try to move 0.005 BTC is quite a hit, basically 20% fee, oof that hurts.

That's why I've been investigating how to get my coins off the exchange with as small loss as possible.
I found out that if you buy other *intermediate* coin that will be used for conversion purpose only and sell it on [sideshift.ai](https://sideshift.ai/), you can minimize your loss.

You don't keep this *intermediate* coin. You buy it, withdraw from Korbit and sell it on sideshift.ai for your target coin moving it to your wallet.

### Why Korbit as main exchange

You might wonder why would you even use Korbit, if there are so many cool exchanges like Coinbase, Kraken, Binance, you name it.
The thing is, all these exchanges are not available in South Korea, you can only use about 4 approved exchanges and Korbit is one of them.

### Why sideshift.ai as exchange for conversion

As it was said, there isn't much choice in South Korea when it comes to crypto exchanges.
I guess you can try and use something like Kraken and transfer you coins there, then convert them and withdraw from Kraken, but to send to
Kraken you have to abide Korbit Travel Rule and wait for Kraken account approval on Korbit account.

You also have to follow Travel Rule for your own wallets, but applying for screening for wallets are more straightforward in my opinion.

sideshift.ai is quite easy to use and has no KYC (Know You Customer) policy.

There are other exchanges that have no KYC policy, but they seem to have higher fees.

## Preview

That's what you get. Not bad, huh?

![2022-07-01 12_05_30-Windows PowerShell](https://user-images.githubusercontent.com/17357759/176815654-c4b288f4-5e81-4fa2-934b-ca4dcd727734.png)

## Getting started

It's a cross-platform application that you can run on any platform that has .NET 6 runtime available.

### Binaries

In most cases you should just grab the latest available release.
You can find pre-build binaries for Windows, Mac and Linux in [Releases](https://github.com/ipasechnikov/KorbitSideShiftCryptoConverter/releases) section.

Don't forget to make sure that you have .NET Runtime installed on your machine. You can get latest .NET Runtime from [Microsoft](https://www.microsoft.com/net/download/).

### Building

If you planning to build application from source code then you have to install .NET SDK at first. It's available for download over [here](https://www.microsoft.com/net/download/).

To build an app, simply run the following command from the root of the solution:

```console
dotnet build
```

### Running

Running process can't get any simpler than this:

1. Input how much money in KRW you want to spend on your crypto. That's the money that you buy crypto on Korbit with. You can format it with commas for example `1,000,000` or `1,000,000,000`.
2. Select your target crypto. That's the crypto you want to convert to and put it in your wallet at the end of the process.

Or you can skip input for both of these values and use default value by pressing Enter.

After you inputted all the data the app will start to convert your money in a loop with a frequency interval of 10 seconds. It will request current exchange rates from Korbit and sideshift.ai via their public APIs and print a table showing how much of your target coin you'll get in case you convert though one or another coin.

There are not much underlying checks going on. In case of bad user input data, app will simply crash. To stop an app you can press Ctrl+C. It's wild, I know.

### Available coins

There are not many coins available in the app, the reasons for that are withdrawal fees and availability on both exchanges.

- sideshift.ai has quite a restricted list of support coins
- There are quite a few coins that have reasonable withdrawal fees

This coin is basically the reason why this tool was create in the first place, it has unreasonable high withdrawal fees. That's why this coin is selected by default.

- BCH

These coins are good as intermediate coins for conversion. They have quite a low withdrawal fee.

- ADA
- AVAX
- AXS
- COMP
- DOGE
- FTM
- MATIC
- SHIB
- SOL
- XLM
- XRP
- YFI

These 2 stablecoins are not reasonable for now, but if the price of other coins goes up, stablecoin can provide lesser fees.

- DAI
- USDC

### Modifying

You can freely modify the code and add your own coins, change fees, etc. You can even add your own exchange and use its API.

## Contributing

I don't think that this project is good for contributing, but if you have any idea, proposal or issues, please open an issue or create a pull request.

It shouldn't be much of an issue to understand most of the code, but if you have any problems, please open an issue or create a pull request.

Code quality probably is not the best. It's a small project written in a few hours in my free time. It utilizes a .NET 6 [new console app template](https://aka.ms/new-console-template), so Program.cs file looks like some scripting language.

## License

The project is available as open source under the terms of the [MIT License](https://opensource.org/licenses/MIT).
