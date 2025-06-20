<!-- Markdown提示/错误等：https://github.com/orgs/community/discussions/16925-->


<div align="center">
<h1>Split archive</h1>

[中文](https://github.com/1234567Yang/SplitArchive) [English](https://github-com.translate.goog/1234567Yang/SplitArchive?_x_tr_sl=zh-CN&_x_tr_tl=en&_x_tr_hl=zh-CN&_x_tr_pto=wapp)

</div>
<br>

还在为 Cloudflare pages 文件最高 25MB 发愁？不如试试分卷压缩！将一个文件分成不同的小块，实现大文件照常上传。
<br>

### 示意图：

#### 分割：
```
[--------------------------------------big_size_file--------------------------------------]
                                            ↓ SplitArchive
[----small_file_1----] [----small_file_2----] [----small_file_3----] [----small_file_4----][original_big_file_hash]
```

#### 合并：
```
[----small_file_1----] [----small_file_2----] [----small_file_3----] [----small_file_4----][original_big_file_hash]
                                            ↓ CombineBack
[--------------------------------------big_file_size--------------------------------------] ? Hash correct ?
                                            ↓ Verify hash       -> Not correct: Warning
[--------------------------------------big_file_size--------------------------------------]
```



<br>

~~我们的宗旨：~~
* ~~薅秃 Cloudflare~~
* ~~把 Cloudflare pages 当不限速网盘用~~

<br>

> [!NOTE]  
> 对于大于 25MB 的视频文件，有时间我会写一个把 MP4 转为 m3u8 的程序。


分卷压缩：
<img src="img/split.png">
<br>
从网络上下载：
<img src="img/download.png">
<br>
还原：
<img src="img/combine.png">
