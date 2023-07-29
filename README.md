# SniffLog
SniffLog lets user sniff and translate Modbus communication in a RS-485 network between multiple device.

![Immagine 2023-07-29 081220](https://github.com/fasterbicio/SniffLog/assets/58078642/75c22944-b085-4e30-ad08-08f40e75760c)

It gives user the ability to toggle on/off Modbus communication verbal translation to easily understand the meaning of PDUs.

![Immagine 2023-07-29 080901](https://github.com/fasterbicio/SniffLog/assets/58078642/3c2a0e1b-d4f9-4a00-b9aa-37b5b023b947)

Communication log can be easily saved in .txt file for further analysis

# Catch
The "Catch" feature enables the user to highlight or isolate the communication with a single slave, in case of a crowded network.
Also, user can highlight/isolate a precise function code, in case of search of a precise event.
The highlight button toggles isolation of the only catch PDUs.

![Immagine 2023-07-29 081803](https://github.com/fasterbicio/SniffLog/assets/58078642/9f0c5e5c-74fa-488a-b3f4-9317b5061563)

Both catch Slave address and catch Function Code are in decimal notaion.

# Requirements
SniffLog runs in .NET framework 4.8.1.
