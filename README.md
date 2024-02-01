# re-phigros

## 简介

**RE:Phigros（又名Phi:Re）的主项目**

## 代码标准

**严格地**遵循C#的标准代码风格。

- 在变量名称与参数名称上使用小驼峰命名法
- 即使有些情况下，大括号不是特别重要的，也要总是使用
- 使用四个空格占位，而不是tab
- 尽量使用`var`，以尽可能减少杂乱的现象
- 保持每行长度在100字符以内

**永远不要**使用`public`向Unity公开成员，用`[SerializeField]`代替。

## Git标准

**永远不要**直接commit到`master`分支。

**永远不要**尝试在未提出pull request的情况下merge一个分支。

## 616.sb发布
发布windows和apk，apk后缀为apk即可，windows的zip命名规则为RPGR_{版本号}_Windows.zip, tag名与版本号一致

## 其他
RPGREncryptHelper.exe: 用于外部加密、解密角色package的程序，里面存有RSA私钥，不要泄露。源码在另一个仓库